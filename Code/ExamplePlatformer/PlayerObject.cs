using MonoFrostTemplate.Code.Utils.Visual;
using System;

namespace MonoFrostTemplate.Code.ExamplePlatformer
{
    public class PlayerObject : GameObject
    {
        public float walkAccel = 0.75f;
        public float airAccel = 0.25f;
        public float walkSpeed = 5f;
        public float jumpSpeed = -12;
        private bool jumping = false;
        private byte coyoteTimer = 0;

        public Controller input;

        Dictionary<string, Animation> anims = new()
        {
            { "idle", new Animation("ExampleContent/PlatformPlayer_Idle", 2, 20, new Vector2(16), true) },
            { "walk", new Animation("ExampleContent/PlatformPlayer_Walk", 6, 6, new Vector2(16), true) },
            { "jump", new Animation("ExampleContent/PlatformPlayer_Jump", 2, 0, new Vector2(16), true) }
        };

        public PlayerObject()
        {
            animation = anims["idle"];
            gravity = 0.75f;
            fallSpeed = 12;

            input = new Controller(0);

            collider = new Collider(this, 32, 48, new Vector2(-16, -16));
        }

        public override void Update()
        {
            int xDir = 0;
            if (input.KeyDown(Controller.Key_Right)) xDir++;
            if (input.KeyDown(Controller.Key_Left)) xDir--;
            if (xDir != 0) direction = xDir;

            float appliedAccel = Grounded ? walkAccel : airAccel;
            if (xDir == -Math.Sign(velocity.X)) appliedAccel *= 4;

            velocity.X.Approach(walkSpeed * xDir, appliedAccel);

            if (Grounded) coyoteTimer = 6;
            else if (coyoteTimer > 0) coyoteTimer--;

            if ((Grounded || (coyoteTimer > 0 && !input.KeyDown(Controller.Key_Down))) && input.ReceiveKeyPress(Controller.Key_Jump))
            {
                velocity.Y = jumpSpeed;
                jumping = true;
                coyoteTimer = 0;
            }
            if (jumping)
            {
                velocity.Y -= gravity / 3;
                if (velocity.Y > -1) jumping = false;
                if (!input.KeyDown(Controller.Key_Jump))
                {
                    jumping = false;
                    velocity.Y /= 1.25f;
                }
            }

            UpdateGravity();
            UpdatePosition();


            if (!Grounded)
            {
                animation = anims["jump"];
                if (velocity.Y < 0) animation.currentFrame = 1;
                else animation.currentFrame = 2;
            }
            else if (xDir != 0) animation = anims["walk"];
            else animation = anims["idle"];

            foreach (Animation a in anims.Values) if (animation != a) a.Reset();

            base.Update();
        }

        public override void AddLights()
        {
            scene.AddPointLight(position, 320, Color.White, 0.5f, 0.5f, 0.5f);
        }

        public void Reset()
        {
            velocity = Vector2.Zero;
            jumping = false;
            coyoteTimer = 0;
            input.ClearAllInputs();
        }

        public override bool FallThroughPlatforms => input.KeyDown(Controller.Key_Down) && input.KeyPressed(Controller.Key_Jump);
    }
}
