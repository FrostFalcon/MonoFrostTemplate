using MonoFrostTemplate.Code.Utils.Visual;

namespace MonoFrostTemplate.Code.Utils.Combat
{
    public class Attack
    {
        public Animation animation;
        public int duration;
        public string name = "";
        /// <summary>
        /// <para>If 0, the attack works in the air and on the ground.</para>
        /// <para>If 1, only works on the ground.</para>
        /// <para>If 2, only works in the air.</para>
        /// <para>0 by default</para>
        /// </summary>
        public int groundedness = 0;
        /// <summary>
        /// <para>Data structure to spawn hitboxes during the move</para>
        /// <para>null by default</para>
        /// </summary>
        public HitboxSpawner[] hitboxes;
        /// <summary>
        /// <para>Data structure to apply velocity during the move</para>
        /// <para>null by default</para>
        /// </summary>
        public Pusher[] pushers;


        public Attack(Animation animation)
        {
            this.animation = animation;
            duration = animation.frames * animation.frameRate;
        }
    }

    public class HitboxSpawner
    {
        protected GameObject owner;
        /// <summary>
        /// <para>The frame of the attack when the hitbox spawns</para>
        /// <para>-1 by default, meaning it won't spawn</para>
        /// </summary>
        public int creationFrame = -1;
        /// <summary>
        /// <para>The damage that the hitbox deals</para>
        /// <para>1 by default</para>
        /// </summary>
        public int damage = 1;
        /// <summary>
        /// <para>Whether the hitbox will hit the player or enemies</para>
        /// <para>false by default</para>
        /// </summary>
        public bool friendly = false;
        /// <summary>
        /// <para>The knockback that the hitbox deals</para>
        /// <para>No knockback by default</para>
        /// </summary>
        public Knockback knockback = new Knockback(0, 0, false);
        /// <summary>
        /// <para>How many entities the hitbox can hit</para>
        /// <para>1 by default</para>
        /// </summary>
        public int pierce = 1;
        /// <summary>
        /// <para>How long the enemy is stunned after being hit</para>
        /// <para>24 frames by default</para>
        /// </summary>
        public int hitstun = 24;
        /// <summary>
        /// <para>The amount of frames the hitbox lasts</para>
        /// <para>3 by default</para>
        /// </summary>
        public int lifetime = 3;
        /// <summary>
        /// <para>The position where the hitbox spawns relative to the player</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 offset = Vector2.Zero;
        /// <summary>
        /// <para>The size of the hitbox</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 size = Vector2.Zero;
        /// <summary>
        /// <para>URL for the sound that plays when the hitbox hits</para>
        /// <para>No hit sound by default</para>
        /// </summary>
        public string hitSound = "";
        /// <summary>
        /// <para>Methods to run when the hitbox hits an opponent</para>
        /// <para>Hit effects are run before damage is applied, so it can be used to modify properties like damage and knockback</para>
        /// <para>null by default</para>
        /// </summary>
        public List<Hitbox.HitEffect> onHitEffects = null;
        /// <summary>
        /// <para>Whether the hitbox can hit the enemy</para>
        /// <para>true by default</para>
        /// </summary>
        public bool active = true;

        public HitboxSpawner(GameObject owner)
        {
            this.owner = owner;
        }

        public virtual void SpawnHitBox()
        {
            new Hitbox(owner, (int)size.X, (int)size.Y, offset)
            {
                damage = damage,
                kb = knockback,
                pierce = pierce,
                lifetime = lifetime,
                hitSFX = hitSound != "" ? Game.LoadAsset<SoundEffect>(hitSound) : null,
                hitEffects = onHitEffects,
                friendly = friendly
            };
        }
    }

    public class ProjectileSpawner : HitboxSpawner
    {
        /// <summary>
        /// Sprite for the projectile
        /// </summary>
        public string sprite;
        /// <summary>
        /// Animation for the projectile
        /// </summary>
        public Animation anim = null;
        /// <summary>
        /// <para>A multiplier for the sprite to be drawn</para>
        /// <para>4 by default</para>
        /// </summary>
        public float spriteScale = 1;
        /// <summary>
        /// <para>Velocity for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public Vector2 velocity = Vector2.Zero;
        /// <summary>
        /// <para>Whether the projectile faces in the driection it's moving when it spawns</para>
        /// <para>false by default</para>
        /// </summary>
        public bool rotateToVelocity = false;
        /// <summary>
        /// <para>Gravity for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public float gravity = 0;
        /// <summary>
        /// <para>Max fall speed for the projectile when it spawns</para>
        /// <para>0 by default</para>
        /// </summary>
        public float fallSpeed = 0;
        /// <summary>
        /// <para>Whether the projectile will destroy itself when touching a tile</para>
        /// <para>true by default</para>
        /// </summary>
        public bool ignoreTiles = true;
        /// <summary>
        /// <para>Update method that will be attached to the projectile</para>
        /// </summary>
        public Projectile.ProjectileUpdate projectileUpdate = null;

        public ProjectileSpawner(GameObject owner, string sprite) : base(owner)
        {
            this.sprite = sprite;
        }

        public ProjectileSpawner(GameObject owner, Animation animation) : base(owner)
        {
            anim = animation;
        }

        public override void SpawnHitBox()
        {
            if (sprite != null) new Projectile(owner, sprite, (int)size.X, (int)size.Y, offset)
            {
                damage = damage,
                kb = knockback,
                pierce = pierce,
                lifetime = lifetime,
                hitSFX = hitSound != "" ? Game.LoadAsset<SoundEffect>(hitSound) : null,
                hitEffects = onHitEffects,
                drawScale = spriteScale,
                velocity = new Vector2(velocity.X * owner.direction, velocity.Y),
                gravity = gravity,
                fallSpeed = fallSpeed,
                ignoreTiles = ignoreTiles,
                projectileUpdate = projectileUpdate,
                friendly = friendly
            };
            else if (anim != null) new Projectile(owner, anim, (int)size.X, (int)size.Y, offset)
            {
                damage = damage,
                kb = knockback,
                pierce = pierce,
                lifetime = lifetime,
                hitSFX = hitSound != "" ? Game.LoadAsset<SoundEffect>(hitSound) : null,
                hitEffects = onHitEffects,
                drawScale = spriteScale,
                velocity = new Vector2(velocity.X * owner.direction, velocity.Y),
                gravity = gravity,
                fallSpeed = fallSpeed,
                ignoreTiles = ignoreTiles,
                projectileUpdate = projectileUpdate,
                friendly = friendly
            };
        }
    }

    public struct Pusher
    {
        public int frame;
        public Vector2 vel;

        public Pusher(int pushFrame, Vector2 velocity)
        {
            frame = pushFrame;
            vel = velocity;
        }

        public void Push(GameObject obj)
        {
            obj.velocity = vel;
            obj.velocity.X *= obj.direction;
        }
    }

    public struct Knockback
    {
        public float baseKB;
        public float angle;
        public bool groundLaunch;

        public Knockback(float baseKnockBack, float knockBackAngle, bool launchFromGround = false)
        {
            baseKB = baseKnockBack;
            angle = knockBackAngle;
            groundLaunch = launchFromGround;
        }
    }
}
