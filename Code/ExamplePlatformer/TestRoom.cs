namespace MonoFrostTemplate.Code.ExamplePlatformer
{
    public class TestRoom : Room
    {
        public TestRoom()
        {
            TileToMap floor = new TileToMap()
            {
                sprite = "ExampleContent/FloorTile",
                solid = true
            };
            TileToMap plat = new TileToMap()
            {
                sprite = "ExampleContent/PlatformTile",
                solid = true,
                platform = true
            };

            Dictionary<Color, TileToMap> colorDictionary = new Dictionary<Color, TileToMap>()
            {
                {new Color(64, 64, 64), floor },
                {new Color(128, 128, 128), plat },
                {new Color(0, 255, 0), new ObjectSpawner() { objectType = typeof(PlayerObject) } },
            };

            map = TileMap.TileMapFromPNG("ExampleContent/PlatformerTilemap", colorDictionary);

            bounds = new Rectangle(0, 0, (map.GetLength(1) - 1) * TileMap.Tile_Size, (map.GetLength(0) - 1) * TileMap.Tile_Size);

            TileMap.GenerateMap(map);
        }
    }
}
