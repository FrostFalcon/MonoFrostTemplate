using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoFrostTemplate.Code.Utils.Map
{
    public class RoomTransition : LevelObject
    {
        public Direction entryDirection;
        public string destination;
        public int ejectDirection;
        public bool exitOnly;
        public int warpID;
        public int exitWarpID;

        public RoomTransition(Vector2 size, string direction)
        {
            switch (direction)
            {
                case "Up":
                    entryDirection = Direction.Up;
                    break;
                case "Down":
                    entryDirection = Direction.Down;
                    break;
                case "Left":
                    entryDirection = Direction.Left;
                    break;
                default:
                    entryDirection = Direction.Right;
                    break;
            }

            collider = new Collider(this, size.X + 44, size.Y + 16, new Vector2(TileMap.Tile_Size / -2 - 8));
            updateOffscreen = true;
        }

        public override void Update()
        {
            if (exitOnly) return;

            if (TouchingObject(collider.Box, scene.player) && EntryCheck(scene.player) && game.screenTransition == null)
            {
                scene.player.input.ClearAllInputs();
                if (entryDirection == Direction.Right) scene.player.input.PressKey(Controller.Key_Right);
                if (entryDirection == Direction.Left) scene.player.input.PressKey(Controller.Key_Left);
                Game.Instance.screenTransition = new CircleTransition()
                {
                    duration = 40,
                    transitionTime = 20,
                    target = position + new Vector2(-32, 0),
                    onTransition = () => scene.LoadLevel(destination, default, this)
                };
            }
        }

        private bool EntryCheck(PlayerObject player)
        {
            if (entryDirection == Direction.Up) return player.position.Y < position.Y && player.velocity.Y < -6;
            if (entryDirection == Direction.Down) return player.position.Y > position.Y && player.velocity.Y > 0;
            if (entryDirection == Direction.Left) return player.position.X < position.X && player.velocity.X < 0;
            if (entryDirection == Direction.Right) return player.position.X > position.X && player.velocity.X > 0;
            return false;
        }

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}
