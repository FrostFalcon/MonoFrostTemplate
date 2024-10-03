using System.Text.Json;

namespace MonoFrostTemplate.Code.Utils.Map
{
    public static class LevelDecoder
    {
        const int PixScale = 4;
        public static Rectangle lastBounds;
        private static int levelStateID = 0;
        private static string currentLevelID;

        static Dictionary<string, Action<InGame, Vector2, Vector2, Dictionary<string, string>>> objectSpawners = new()
        {
            { "Player", (scene, pos, size, fld) =>
            {
                if (scene.player == null)
                {
                    scene.player = new PlayerObject() {position = pos};
                    scene.loadedObjects.Add(scene.player);
                }
            }},
            { "RoomTransition", (scene, pos, size, fld) =>
            {
                RoomTransition rt = new RoomTransition(size, fld["Direction"].Replace("\"", "")) { position = pos };
                scene.loadedObjects.Add(rt);
                rt.destination = fld["Destination"].Replace("\"", "");
                rt.warpID = int.Parse(fld["WarpID"]);
                rt.exitWarpID = int.Parse(fld["ExitWarpID"]);
                rt.ejectDirection = int.Parse(fld["EjectDirection"]);
                rt.exitOnly = bool.Parse(fld["ExitOnly"]);
                rt.levelStateID = levelStateID;
            }}
        };

        static Dictionary<string, TileToMap> tilesets = new Dictionary<string, TileToMap>()
        {
            { "Tiles", new TileToMap() { solid = true, platform = false, sourceTexture = "ExampleContent/FloorTile" } },
            { "Platforms", new TileToMap() { solid = true, platform = true, sourceTexture = "ExampleContent/PlatformTile" } },
        };

        public static void LoadLevel(InGame scene, string worldFile, string levelID)
        {
            using (StreamReader r = new StreamReader("Assets/Levels/" + worldFile + ".ldtk"))
            {
                currentLevelID = levelID;
                levelStateID = 0;

                string json = r.ReadToEnd();
                JsonDocument doc = JsonDocument.Parse(json);

                JsonElement level = doc.RootElement.GetProperty("levels").EnumerateArray().ToList().First(e => e.GetProperty("identifier").GetString() == levelID);
                lastBounds = new Rectangle(4, 4, level.GetProperty("pxWid").GetInt32() * PixScale - 8, level.GetProperty("pxHei").GetInt32() * PixScale - 8);
                scene.tiles = new Tile[level.GetProperty("pxHei").GetInt32() * PixScale / TileMap.Tile_Size, level.GetProperty("pxWid").GetInt32() * PixScale / TileMap.Tile_Size];

                foreach (JsonElement e in level.GetProperty("layerInstances").EnumerateArray())
                {
                    string id = e.GetProperty("__identifier").GetString();
                    string type = e.GetProperty("__type").GetString();
                    if (tilesets.ContainsKey(id))
                    {
                        foreach (JsonElement t in e.GetProperty("autoLayerTiles").EnumerateArray())
                        {
                            int x = t.GetProperty("px")[0].GetInt32() * PixScale / TileMap.Tile_Size;
                            int y = t.GetProperty("px")[1].GetInt32() * PixScale / TileMap.Tile_Size;
                            scene.tiles[y, x] = new Tile("", x * TileMap.Tile_Size + TileMap.Tile_Size / 2, y * TileMap.Tile_Size + TileMap.Tile_Size / 2)
                            {
                                sourceTexture = Game.LoadAsset<Texture2D>(tilesets[id].sourceTexture),
                                sourceRectangle = new Rectangle(t.GetProperty("src")[0].GetInt32(), t.GetProperty("src")[1].GetInt32(), 16, 16),
                                forceCollision = tilesets[id].solid,
                                platform = tilesets[id].platform,
                            };
                        }
                    }

                    if (type == "Entities")
                    {
                        foreach (JsonElement obj in e.GetProperty("entityInstances").EnumerateArray())
                        {
                            string id2 = obj.GetProperty("__identifier").GetString();
                            if (objectSpawners.ContainsKey(id2))
                            {
                                Dictionary<string, string> fields = new();
                                foreach (JsonElement f in obj.GetProperty("fieldInstances").EnumerateArray())
                                {
                                    fields.Add(f.GetProperty("__identifier").GetString(), f.GetProperty("__value").GetRawText());
                                }
                                objectSpawners[id2].Invoke(scene, new Vector2(obj.GetProperty("__grid")[0].GetInt32() * TileMap.Tile_Size + TileMap.Tile_Size / 2, obj.GetProperty("__grid")[1].GetInt32() * TileMap.Tile_Size + TileMap.Tile_Size / 2),
                                    new Vector2(obj.GetProperty("width").GetInt32() * PixScale, obj.GetProperty("height").GetInt32() * PixScale), fields);
                                levelStateID++;
                            }
                        }
                    }
                }
            }
        }
    }
}
