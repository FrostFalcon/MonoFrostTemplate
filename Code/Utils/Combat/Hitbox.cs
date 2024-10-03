using System;
using MonoFrostTemplate.Code.Utils.Visual;

namespace MonoFrostTemplate.Code.Utils.Combat
{
    public class Hitbox : GameObject
    {
        public int damage = 1;
        public int lifetime = 1;
        public bool active = true;
        public bool friendly = true;
        public Knockback kb;
        public int pierce = 1;
        public GameObject owner;
        public Vector2 offset;
        public List<GameObject> hitObjects = new List<GameObject>();

        public delegate void HitEffect(Hitbox h, GameObject target);
        public List<HitEffect> hitEffects;
        public SoundEffect hitSFX;

        public Hitbox(GameObject owner, int width, int height, Vector2 positionOffset)
        {
            scene.loadedObjects.Add(this);

            this.owner = owner;
            collider = new Collider(this, width, height, new Vector2(-width / 2, -height / 2));
            offset = positionOffset;

            position = owner.position + offset;
            if (owner.direction == -1) position.X -= (offset.X * 2);
        }

        public override void Update()
        {
            lifetime--;

            if (lifetime <= 0 || pierce <= 0)
            {
                Destroy();
            }

            position = owner.position + offset;
            if (owner.direction == -1) position.X -= (offset.X * 2);

            if (active)
            {
                if (friendly)
                {
                    //Implement after creating an enemy class

                    //foreach (Enemy e in scene.GetObjectsOfType<Enemy>())
                    //{
                    //    if (TouchingObject(collider.Box, e) && !hitObjects.Contains(e))
                    //    {
                    //        HitObject(e, damage);
                    //    }
                    //}
                }
                else
                {
                    if (TouchingObject(collider.Box, scene.player) && !hitObjects.Contains(scene.player))
                    {
                        HitObject(scene.player, damage);
                    }
                }
            }
        }

        public virtual void HitObject(GameObject target, int damage)
        {
            pierce--;
            if (hitEffects != null && hitEffects.Count != 0) foreach (HitEffect e in hitEffects) if (e != null) e(this, target);
            if (hitSFX != null) scene.PlayWorldSound(hitSFX, position, 0.5f);
            hitObjects.Add(target);

            //Implement after creating an enemy class

            //if (target is Player p && p.Iframes == 0)
            //{
            //    p.health -= damage;
            //    p.OnHurt(this);
            //    if (p.power <= 0) p.OnDeath();
            //    hitObjects.Add(p);
            //}
            //if (target is Enemy e)
            //{
            //    e.health -= damage;
            //    e.OnHurt(this);
            //    if (e.health <= 0) e.OnDeath();
            //    hitObjects.Add(e);
            //}
        }
    }

    public class Projectile : Hitbox
    {
        public delegate void ProjectileUpdate(Projectile projectile);
        public ProjectileUpdate projectileUpdate;

        public int timer;

        public ParticleSpawner deathParticle;

        public Projectile(GameObject owner, string sprite, int width, int height, Vector2 positionOffset) : base(owner, width, height, positionOffset)
        {
            this.animation = new Animation(sprite);
            position = owner.position + positionOffset;
        }

        public Projectile(GameObject owner, Animation animation, int width, int height, Vector2 positionOffset) : base(owner, width, height, positionOffset)
        {
            this.animation = new Animation(animation.spriteSheet, animation.frames, animation.frameRate, animation.cellSize, animation.loopAnim)
            {
                sounds = animation.sounds,
                particles = animation.particles
            };
            position = owner.position + positionOffset;
        }

        public override void Update()
        {
            lifetime--;

            if (animation != null && animation.frames > 1)
            {
                animation.updateAnimation();

                if (animation.timer == 0)
                {
                    if (animation.sounds != null) foreach (AnimationSound s in animation.sounds)
                        {
                            if (s.AnimationFrame == animation.currentFrame) scene.PlayWorldSound(s.Sound, position, s.Volume);
                        }

                    if (animation.particles != null) foreach (AnimationParticle p in animation.particles)
                        {
                            if (p.AnimationFrame == animation.currentFrame) new Particle(new Animation(p.ParticleAnimation.spriteSheet, p.ParticleAnimation.frames, p.ParticleAnimation.frameRate, p.ParticleAnimation.cellSize, p.ParticleAnimation.loopAnim), position, p.Lifetime, Vector2.Zero, 1, 0, direction);
                        }
                }
            }

            if (lifetime <= 0 || pierce <= 0 || (!ignoreTiles && (TouchingTile(collider.Box, (int)(velocity.X * 1.2f), 0) || TouchingTile(collider.Box, 0, (int)(velocity.Y * 1.2f)))))
            {
                deathParticle?.Spawn(position, 0);
                Destroy();
            }

            if (projectileUpdate != null) projectileUpdate(this);

            UpdateGravity();
            UpdatePosition();

            if (active)
            {
                if (friendly)
                {
                    //Implement after creating an enemy class

                    //foreach (Enemy e in scene.GetObjectsOfType<Enemy>())
                    //{
                    //    if (TouchingObject(collider.Box, e) && !hitObjects.Contains(e))
                    //    {
                    //        HitObject(e, damage);
                    //    }
                    //}
                }
                else
                {
                    if (TouchingObject(collider.Box, scene.player) && !hitObjects.Contains(scene.player))
                    {
                        HitObject(scene.player, damage);
                    }
                }
            }
        }
    }
}
