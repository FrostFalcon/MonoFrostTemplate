using MonoFrostTemplate.Code.Utils.Visual;

namespace MonoFrostTemplate.Code.Utils.Objects
{
    public class Particle : GameObject
    {
        public int duration;
        public float acceleration;
        public bool updateDuringHitpause = true;
        public GameObject stayWithOwner = null;

        public Particle(Animation animation, Vector2 position, int lifetime, Vector2 velocity, float acceleration = 1, float rotation = 0, int direction = 0) : base()
        {
            drawLayer = 0.9f + game.randomLayerOffset;
            this.animation = animation;
            this.position = position;
            duration = lifetime;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.rotation = rotation;
            this.direction = direction;

            scene.particles.Add(this);
        }

        public override void Update()
        {
            base.Update();
            duration--;

            if (stayWithOwner != null)
            {
                position = stayWithOwner.position;
            }
            else
            {
                velocity *= acceleration;
                position += velocity;
            }

            if (duration <= 0)
            {
                Destroy();
            }
        }
    }

    public class ParticleSpawner
    {
        public Animation animation;
        public int duration;
        public Vector2 velocity;
        public float acceleration;
        public float rotation = 0;
        public GameObject stayWithOwner;

        public ParticleSpawner(Animation animation, int duration, Vector2 velocity, float acceleration = 1, float rotation = 0)
        {
            this.animation = animation;
            this.duration = duration;
            this.velocity = velocity;
            this.acceleration = acceleration;
            this.rotation = rotation;
        }

        public Particle Spawn(Vector2 position, int direction)
        {
            return new Particle(new Animation(animation.spriteSheet, animation.frames, animation.frameRate, animation.cellSize, animation.loopAnim), position, duration, velocity, acceleration, rotation, direction) { stayWithOwner = stayWithOwner };
        }
    }
}
