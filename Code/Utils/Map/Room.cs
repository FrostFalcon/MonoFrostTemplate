namespace MonoFrostTemplate.Code.Utils.Map
{
    /// <summary>
    /// <para>Contains the tilemap, camera bounds, and a draw function that get called by InGame</para>
    /// <para>Should be derived to setup the environment</para>
    /// </summary>
    public class Room
    {
        public Rectangle bounds;

        public TileToMap[,] map;

        public Vector2 spawn = Vector2.Zero;

        public void SetupTiles() => TileMap.GenerateMap(map);

        /// <summary>
        /// A larger number means the background will scroll slower
        /// </summary>
        public float paralaxDistance = 8;

        public virtual void Draw(SpriteBatch spritebatch) { }
    }
}
