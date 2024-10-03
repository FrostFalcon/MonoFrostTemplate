namespace MonoFrostTemplate.Code.Utils.Visual
{
    /// <summary>
    /// <para>Basic class for breaking up a sprite sheet and drawing the corresponding frame</para>
    /// <para>Contains built in sound and partical attachments that can be invoked elsewhere</para>
    /// </summary>
    public class Animation
    {
        /// <summary>
        /// Multiplier applied to the draw scale of every animation drawn
        /// </summary>
        public const float BaseDrawScale = 4;

        public Texture2D spriteSheet;
        public int currentFrame = 1;
        public int frames = 1;
        public int frameRate = 10;
        public int timer = 0;
        public bool loopAnim = true;
        public Vector2 cellSize = new Vector2(1024, 1024);
        public AnimationSound[] sounds;
        public AnimationParticle[] particles;

        public Animation() { }

        public Animation(string spriteSheetURL)
        {
            spriteSheet = Game.LoadAsset<Texture2D>(spriteSheetURL);
            frames = 1;
            frameRate = 0;
            cellSize = new Vector2(spriteSheet.Width, spriteSheet.Height);
        }

        public Animation(Texture2D spriteSheet)
        {
            this.spriteSheet = spriteSheet;
            frames = 1;
            frameRate = 0;
            cellSize = new Vector2(spriteSheet.Width, spriteSheet.Height);
        }

        public Animation(string spriteSheetURL, int frameCount, int frameRate, Vector2 cellSize, bool loop = true)
        {
            spriteSheet = Game.LoadAsset<Texture2D>(spriteSheetURL);
            frames = frameCount;
            this.frameRate = frameRate;
            this.cellSize = cellSize;
            loopAnim = loop;
        }

        public Animation(Texture2D spriteSheet, int frameCount, int frameRate, Vector2 cellSize, bool loop = true)
        {
            this.spriteSheet = spriteSheet;
            frames = frameCount;
            this.frameRate = frameRate;
            this.cellSize = cellSize;
            loopAnim = loop;
        }

        public Animation(string spriteSheetURL, int frameCount, int frameRate, bool loop = true)
        {
            this.spriteSheet = Game.LoadAsset<Texture2D>(spriteSheetURL);
            this.frames = frameCount;
            this.frameRate = frameRate;
            this.cellSize = new Vector2(this.spriteSheet.Width / frameCount, this.spriteSheet.Height);
            loopAnim = loop;
        }

        public Animation(Texture2D spriteSheet, int frameCount, int frameRate, bool loop = true)
        {
            this.spriteSheet = spriteSheet;
            this.frames = frameCount;
            this.frameRate = frameRate;
            this.cellSize = new Vector2(this.spriteSheet.Width / frameCount, this.spriteSheet.Height);
            loopAnim = loop;
        }

        public virtual void updateAnimation()
        {
            if (frameRate != 0)
            {
                timer++;

                if (timer >= frameRate)
                {
                    timer = 0;
                    currentFrame++;
                    if (currentFrame > frames)
                    {
                        if (loopAnim) currentFrame = 1;
                        else currentFrame = frames;
                    }
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Vector2 pos, int direction, Color color, float layer = 0.5f, float rotation = 0, float scale = 1)
        {
            spriteBatch.Draw(spriteSheet, pos, new Rectangle((currentFrame - 1) * (int)cellSize.X, 0, (int)cellSize.X, (int)cellSize.Y),
                color, MathHelper.ToRadians(rotation), new Vector2(cellSize.X, cellSize.Y) / 2, scale * BaseDrawScale, direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layer);
        }

        public void Reset()
        {
            currentFrame = 1;
            timer = 0;
        }

        public static void Reset(params Animation[] anims)
        {
            foreach (Animation a in anims)
            {
                a.currentFrame = 1;
                a.timer = 0;
            }
        }
    }

    /// <summary>
    /// Derived from the Animation class, breaks the animation into two parts, the first gets played once, then the rest gets looped
    /// </summary>
    public class CompoundAnimation : Animation
    {
        public int startupFrames;
        public int startupFrameRate;
        public int loopFrames;

        public CompoundAnimation(string spriteSheetURL, int startupFrameCount, int loopFrameCount, int frameRate, Vector2 cellSize, int startupFrameRate = -1) : base(spriteSheetURL, startupFrameCount + loopFrameCount, frameRate, cellSize, true)
        {
            startupFrames = startupFrameCount;
            loopFrames = loopFrameCount;
            frames = startupFrames + loopFrames;
            this.startupFrameRate = startupFrameRate;
        }

        public CompoundAnimation(Texture2D spriteSheet, int startupFrameCount, int loopFrameCount, int frameRate, Vector2 cellSize, int startupFrameRate = -1) : base(spriteSheet, startupFrameCount + loopFrameCount, frameRate, cellSize, true)
        {
            startupFrames = startupFrameCount;
            loopFrames = loopFrameCount;
            frames = startupFrames + loopFrames;
            this.startupFrameRate = startupFrameRate;
        }

        public override void updateAnimation()
        {
            if (frameRate != 0)
            {
                timer++;

                int rate = startupFrameRate != -1 && currentFrame <= startupFrames ? startupFrameRate : frameRate;

                if (timer >= rate)
                {
                    timer = 0;
                    currentFrame++;
                    if (currentFrame > frames) currentFrame = startupFrames + 1;
                }
            }
        }
    }

    public struct AnimationSound
    {
        private SoundEffect sound;
        public SoundEffect Sound { get => sound; }
        private int frame;
        public int AnimationFrame { get => frame; }
        private float volume;
        public float Volume { get => volume; }

        public AnimationSound(string soundURL, int animationFrame, float volumeMultiplier = 1)
        {
            sound = Game.LoadAsset<SoundEffect>(soundURL);
            frame = animationFrame;
            volume = volumeMultiplier;
        }
    }

    public struct AnimationParticle
    {
        private Animation particleAnimation;
        public Animation ParticleAnimation { get => particleAnimation; }
        private int frame;
        public int AnimationFrame { get => frame; }
        private int lifetime;
        public int Lifetime => lifetime;
        public Vector2 velocity;
        public float acceleration;
        public bool stayWithOwner;

        public AnimationParticle(Animation particleAnim, int animationFrame, int lifetime, float xVel = 0, float yVel = 0, float accel = 1, bool stayWithOwner = false)
        {
            particleAnimation = particleAnim;
            frame = animationFrame;
            this.lifetime = lifetime;
            velocity = new Vector2(xVel, yVel);
            acceleration = accel;
            this.stayWithOwner = stayWithOwner;
        }
    }
}
