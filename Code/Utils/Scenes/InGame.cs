using Microsoft.Xna.Framework.Graphics;
using MonoFrostTemplate.Code.ExamplePlatformer;
using MonoFrostTemplate.Code.Utils.Visual;

namespace MonoFrostTemplate.Code.Utils.Scenes
{
    public class InGame : Scene
    {
        public List<GameObject> loadedObjects = new List<GameObject>();
        public List<GameObject> solidObjects = new List<GameObject>();
        public Tile[,] tiles;
        public List<Particle> particles = new List<Particle>();
        public PlayerObject player;

        public Room room;
        public string currentLevel;
        private Vector2 playerSpawn = new Vector2(128, 320);
        private Vector2 spawnVelocity = Vector2.Zero;
        public Dictionary<(string, int), byte[]> levelStates = new Dictionary<(string, int), byte[]>();

        public Color ambientLight = Color.LightGray;
        public List<Light> lights = new List<Light>();
        RenderTarget2D lightMap;
        RenderTarget2D bloomMap;
        RenderTarget2D sceneRender;
        const float lightScale = 8;

        private int prepareHitpause;
        public int hitpause;
        public float timeScale = 1f;
        private float timeScaleTimer;

        private List<(SoundEffectInstance, Vector2, float)> soundEffects = new();

        private ButtonMatrix pauseMenuUI;
        public bool paused = false;

        public override void Load()
        {
            lightMap = new RenderTarget2D(Game.Instance.GraphicsDevice, (int)(1920 / lightScale), (int)(1080 / lightScale));
            bloomMap = new RenderTarget2D(Game.Instance.GraphicsDevice, 1920, 1080);
            sceneRender = new RenderTarget2D(Game.Instance.GraphicsDevice, 1920, 1080);

            camera = new Camera();
            LoadLevel("Level_0", new Vector2(-1));

            //Setup UI
            UI = new UICanvas();
            SetupPauseMenu();
        }

        public void LoadLevel(string levelID, Vector2 spawn = default, RoomTransition entry = null)
        {
            foreach (LevelObject lo in GetObjectsOfType<LevelObject>()) lo.WriteLevelState(currentLevel);

            loadedObjects.Clear();
            foreach (GameObject o in loadedObjects.ToArray()) if (o is not PlayerObject) o.Destroy();
            particles.Clear();
            solidObjects.Clear();
            if (player != null)
            {
                player.Reset();
                loadedObjects.Add(player);
            }

            LevelDecoder.LoadLevel(this, "TestWorld", levelID);
            room = new Room() { bounds = LevelDecoder.lastBounds };

            if (spawn == default)
            {
                List<RoomTransition> doors = GetObjectsOfType<RoomTransition>();
                RoomTransition rt = doors[0];
                if (entry != null && entry.exitWarpID >= 0)
                {
                    rt = doors.FirstOrDefault(r => r.warpID == entry.exitWarpID);
                }
                else rt = doors.FirstOrDefault(r => r.destination == currentLevel);
                if (rt == null) rt = doors[0];

                playerSpawn = new Vector2(rt.collider.Box.Left + rt.collider.Box.Width / 2,
                    rt.entryDirection == RoomTransition.Direction.Up || rt.entryDirection == RoomTransition.Direction.Down ? rt.collider.Box.Top - player.collider.Box.Height / 2 : rt.collider.Box.Bottom - player.collider.Box.Height / 2);
                player.position = playerSpawn;
                if (rt.ejectDirection != 0) player.direction = rt.ejectDirection;
                if (rt.entryDirection == RoomTransition.Direction.Down) spawnVelocity = new Vector2(6 * rt.ejectDirection, -13);
                else spawnVelocity = Vector2.Zero;
                player.velocity = spawnVelocity;
            }
            else if (spawn != new Vector2(-1))
            {
                playerSpawn = spawn;
                player.position = spawn;
                player.velocity = spawnVelocity;
            }
            camera.Update(true);
        }

        public override void Update(GameTime gametime)
        {
            lights.Clear();
            Game game = Game.Instance;
            if (!paused)
            {
                UpdateControllers();
                UpdateObjects();

                if (hitpause > 0) hitpause--;

                if (player.input.KeyPressed(Controller.Key_Start))
                {
                    player.input.ClearAllBuffers();
                    Pause(player.input.id);
                }
            }
            else if (cursors.Count > 0 && cursors[0].input.KeyPressed(Controller.Key_Start)) Unpause(cursors[0], pauseMenuUI);
            foreach (GameObject o in loadedObjects) if ((camera.target - o.position).Length() < 1920) o.AddLights();
            foreach (Particle p in particles) if ((camera.target - p.position).Length() < 1920) p.AddLights();

            if (prepareHitpause > 0)
            {
                hitpause = Math.Max(hitpause, prepareHitpause);
                prepareHitpause = 0;
            }

            UI.Update();
            UpdateCursors();

            foreach (var se in soundEffects.ToArray())
            {
                if (se.Item1.State == SoundState.Stopped)
                {
                    soundEffects.Remove(se);
                    continue;
                }
                float distanceMultiplier = MathHelper.Clamp(1.5f - ((camera.target - se.Item2).Length() / 1000), 0, 1);
                float vol = Game.SoundEffectVolume * se.Item3 * distanceMultiplier;

                float pan = MathHelper.Clamp((se.Item2.X - camera.target.X) / 1000, -1, 1);
                pan = -1;
                if (vol > 1) vol = 1;

                se.Item1.Volume = Math.Clamp(vol * (16f / soundEffects.Count + 0.25f), 0, 1);
            }
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            //Generate light map
            Texture2D lightSampler = new Texture2D(Game.Instance.GraphicsDevice, 4, Math.Max(lights.Count, 1), false, SurfaceFormat.Vector4);
            ApplyLights(lightSampler);
            Effect effect = Game.LoadAsset<Effect>("Shaders/LightMap");
            effect.Parameters["LightCount"].SetValue(lights.Count);
            effect.Parameters["ambientLight"].SetValue(ambientLight.ToVector4());
            effect.Parameters["screenSize"].SetValue(Vector2.One / camera.zoom * lightScale);
            effect.Parameters["camPos"].SetValue(camera.position);

            Game.Instance.GraphicsDevice.SetRenderTarget(lightMap);
            Game.Instance.GraphicsDevice.Clear(Color.Black);
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, null, null, effect);
            spritebatch.Draw(lightSampler, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.5f);
            spritebatch.End();

            //Generate Bloom
            Game.Instance.GraphicsDevice.SetRenderTarget(bloomMap);
            Game.Instance.GraphicsDevice.Clear(Color.Black);
            spritebatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, null, null, null, camera.viewMatrix);
            foreach (GameObject o in loadedObjects.ToArray()) if ((o.position - camera.target).Length() <= 1920) o.DrawBloom(spritebatch);
            spritebatch.End();

            //Draw objects to scene
            Game.Instance.GraphicsDevice.SetRenderTarget(sceneRender);
            Game.Instance.GraphicsDevice.Clear(Color.SkyBlue);
            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, camera.viewMatrix);
            if (room != null) room.Draw(spritebatch);
            DrawObjects(spritebatch);
            spritebatch.End();

            //Draw scene with light map
            Game.Instance.GraphicsDevice.SetRenderTarget(Game.Instance.mainRender);
            effect = Game.LoadAsset<Effect>("Shaders/LightShader");
            effect.Parameters["LightMap"].SetValue(lightMap);
            effect.Parameters["BloomMap"].SetValue(bloomMap);
            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, effect);
            spritebatch.Draw(sceneRender, Vector2.Zero, Color.White);
            spritebatch.End();

            Texture2D gray = new Texture2D(Game.Instance.GraphicsDevice, 1, 1);
            gray.SetData(new Color[] { new Color(Color.DarkGray, 0.6f) });

            spritebatch.Begin(SpriteSortMode.FrontToBack, BlendState.NonPremultiplied, SamplerState.PointClamp);
            if (paused) spritebatch.Draw(gray, new Rectangle(0, 0, 1920, 1080), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0.1f);
            UI.Draw(spritebatch);
            foreach (MenuCursor c in cursors) c.Draw(spritebatch);
            spritebatch.End();

            lightSampler.Dispose();
        }

        void ApplyLights(Texture2D lightSampler)
        {
            if (lights.Count == 0)
            {
                lightSampler.SetData(new Vector4[]
                {
                    Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero
                });
            }
            else
            {
                Vector4[] c = new Vector4[lights.Count * 4];
                for (int i = 0; i < lights.Count; i++)
                {
                    c[i * 4] = new Vector4(lights[i].position, lights[i].radius, lights[i].type);
                    c[i * 4 + 1] = lights[i].color.ToVector4();
                    c[i * 4 + 2] = new Vector4(lights[i].decay, 0);
                    c[i * 4 + 3] = lights[i].extra;
                }
                lightSampler.SetData(c);
            }
        }

        public void AddPointLight(Vector2 position, float radius, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(0, position, radius, color, new Vector3(redDecay, greenDecay, blueDecay), Vector4.Zero));
        }
        public void AddLineLight(Vector2 start, Vector2 end, float width, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(1, start, width, color, new Vector3(redDecay, greenDecay, blueDecay), new Vector4(end, 0, 0)));
        }
        public void AddConeLight(Vector2 start, float angle, float spread, Color color, float redDecay = 1, float greenDecay = 1, float blueDecay = 1)
        {
            if (lights.Count < 10000) lights.Add(new Light(2, start, MathHelper.ToRadians(spread % 360), color, new Vector3(redDecay, greenDecay, blueDecay), new Vector4(MathHelper.ToRadians(angle % 360), 0, 0, 0)));
        }

        //Gets all objects in loadedObjects that are of type T and its derivitives
        public virtual List<T> GetObjectsOfType<T>()
        {
            List<T> list = new List<T>();

            foreach (GameObject o in loadedObjects) if (o is T t) list.Add(t);

            return list;
        }

        protected virtual void UpdateControllers()
        {
            if (Game.Instance.screenTransition == null)
                player.input.UpdateKeys();
        }

        protected virtual void UpdateObjects()
        {
            if (UI.elements.Exists(e => e is PopupMessage)) return;

            timeScaleTimer += timeScale;
            while (timeScaleTimer >= 1)
            {
                timeScaleTimer--;
                camera.Update();

                foreach (GameObject o in loadedObjects.ToArray()) if (hitpause <= 0 && ((o.position - camera.target).Length() <= 1920 || o.updateOffscreen)) o.Update();
                foreach (Particle p in particles.ToArray()) if (hitpause <= 0 || p.updateDuringHitpause) p.Update();
            }
        }

        protected virtual void DrawObjects(SpriteBatch spriteBatch)
        {
            foreach (GameObject o in loadedObjects.ToArray()) if ((o.position - camera.target).Length() <= 1920) o.Draw(spriteBatch);
            for (int x = (int)(player.position.X / TileMap.Tile_Size) - (camera.CamWidth / TileMap.Tile_Size) - 2; x < (int)(player.position.X / TileMap.Tile_Size) + (camera.CamWidth / TileMap.Tile_Size) + 2; x++)
            {
                for (int y = (int)(player.position.Y / TileMap.Tile_Size) - (camera.CamHeight / TileMap.Tile_Size) - 2; y < (int)(player.position.Y / TileMap.Tile_Size) + (camera.CamHeight / TileMap.Tile_Size) + 2; y++)
                {
                    if (x >= 0 && x < tiles.GetLength(1) && y >= 0 && y < tiles.GetLength(0) && tiles[y, x] != null && !loadedObjects.Contains(tiles[y, x])) tiles[y, x].Draw(spriteBatch);
                }
            }
            foreach (Particle p in particles.ToArray()) p.Draw(spriteBatch);
        }

        public virtual void ApplyHitpause(int frames)
        {
            prepareHitpause = Math.Max(prepareHitpause, frames);
        }

        public virtual void PlayWorldSound(SoundEffect sound, Vector2 position, float volumeMultiplier = 1)
        {
            float distanceMultiplier = MathHelper.Clamp(1.5f - ((camera.target - position).Length() / 1000), 0, 1);
            float vol = Game.SoundEffectVolume * volumeMultiplier * distanceMultiplier;

            float pan = MathHelper.Clamp((position.X - camera.target.X) / 1000, -1, 1);

            if (vol > 1) vol = 1;

            SoundEffectInstance se = sound.CreateInstance();
            soundEffects.Add((se, position, volumeMultiplier));
            se.Volume = Math.Clamp(vol * (16f / soundEffects.Count + 0.25f), 0, 1);
            se.Pan = pan;
            se.Play();
        }

        public virtual void SetupPauseMenu()
        {
            pauseMenuUI = ButtonMatrix.Create1DMatrix(true, 960, 540, 160, new List<UIElement>() {
                new TextBlock("Resume", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton") { onClick = Unpause, size = new Vector2(320, 80) },
                new TextBlock("Quit", "ExampleContent/UIFont", Color.White, "ExampleContent/UIButton") { onClick = (c, m) => Game.Instance.Exit(), size = new Vector2(320, 80) }
            }, null, false);
        }

        public virtual void Pause(int controllerID)
        {
            if (!paused)
            {
                paused = true;
                UI.buttonMatricies.Add(pauseMenuUI);
                cursors.Add(new MenuCursor(controllerID, pauseMenuUI));
                cursors[0].input.ClearAllBuffers();
                foreach (var se in soundEffects) se.Item1.Pause();
            }
        }

        public virtual void Unpause(MenuCursor c, ButtonMatrix m)
        {
            if (paused)
            {
                paused = false;
                foreach (UIElement e in UI.elements) e.hoverScale = 1;
                foreach (ButtonMatrix b in UI.buttonMatricies) foreach (UIElement e in b.buttons) e.hoverScale = 1;
                UI.buttonMatricies.Clear();
                cursors.Clear();

                //Stops the menu input from carrying over into the game
                player.input.UpdateKeys();
                player.input.ClearAllBuffers();
                foreach (var se in soundEffects) se.Item1.Resume();
            }
        }
    }
}