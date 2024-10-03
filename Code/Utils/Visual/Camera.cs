namespace MonoFrostTemplate.Code.Utils.Visual
{
    public class Camera
    {
        public Matrix viewMatrix;

        public Vector2 target;
        public Vector2 position;
        protected Vector2 overridePosition;
        public void ResetOverridePosition() => overridePosition = Vector2.Zero;

        public Rectangle targetBounds = new Rectangle(0, 0, 80000, 80000);

        public int CamWidth => (int)(1920 / zoom);
        public int CamHeight => (int)(1080 / zoom);

        public float zoom = 1f;
        protected float overrideZoom = 0;

        public List<float> shakeAmount = new List<float>();
        public List<int> shakeTimer = new List<int>();

        protected float LerpRate => 0.25f;
        protected float ZoomMax => 1f;
        protected float ZoomMin => 2f;
        protected Vector2 TargetOffset => Vector2.Zero;

        public Camera()
        {
            target = Vector2.Zero;
        }

        public void Update(bool instant = false)
        {
            Game game = Game.Instance;
            if (game.Scene is InGame scene)
            {
                targetBounds = scene.room.bounds;
                List<GameObject> targets = new List<GameObject>();
                if (scene.player != null) targets.Add(scene.player);

                float avgDist = 0;
                if (targets.Count == 0)
                {
                    target = Vector2.Zero;
                }
                else if (targets.Count == 1)
                {
                    target = targets[0].position;
                }
                else
                {
                    target = Vector2.Zero;
                    foreach (GameObject o in targets)
                    {
                        if (o != null) target += o.position;
                    }
                    target /= targets.Count;
                    foreach (GameObject o in targets)
                    {
                        if (o != null) avgDist += (target - o.position).Length();
                    }
                    avgDist /= targets.Count;
                }
                target += TargetOffset;

                if (instant)
                {
                    zoom = 1200 / (avgDist + 200);
                    zoom = MathHelper.Clamp(zoom, 2.8f, 3f);
                }
                else
                {
                    zoom = MathHelper.Lerp(zoom, MathHelper.Clamp(1200 / (avgDist + 200), ZoomMin, ZoomMax), LerpRate);
                }

                if (overridePosition != Vector2.Zero)
                {
                    target = overridePosition;
                }
                if (overrideZoom != 0)
                {
                    zoom = overrideZoom;
                    overrideZoom = 0;
                }
                target.Y -= CamHeight / 12;

                try
                {
                    target.X = MathHelper.Clamp(target.X, targetBounds.Left + CamWidth / 2, targetBounds.Right - CamWidth / 2);
                }
                catch
                {
                    target.X = targetBounds.Center.X;
                }
                target.Y = Math.Max(target.Y, targetBounds.Top + CamHeight / 2);
                target.Y = Math.Min(target.Y, targetBounds.Bottom - CamHeight / 2);

                if (instant) position = target - new Vector2(CamWidth, CamHeight) / 2;
                else position = Vector2.Lerp(position, target - new Vector2(CamWidth, CamHeight) / 2, LerpRate);

                for (int i = shakeAmount.Count - 1; i >= 0; i--)
                {
                    position.Y += MathF.Sin(MathHelper.ToRadians(shakeTimer[i] * 90)) * shakeAmount[i];
                    shakeAmount[i] *= 0.75f;
                    shakeTimer[i]--;
                    if (shakeTimer[i] == 0)
                    {
                        shakeAmount.RemoveAt(i);
                        shakeTimer.RemoveAt(i);
                    }
                }
                position = Vector2.Round(Vector2.Clamp(position, Vector2.Zero, new Vector2(targetBounds.Width - CamWidth, targetBounds.Height - CamHeight)));
            }
            else
            {
                position = Vector2.Zero;
                zoom = 1;
            }

            viewMatrix = Matrix.CreateTranslation(new Vector3(-position, 0)) * Matrix.CreateScale(zoom, zoom, 1);
        }

        public void OverridePosition(Vector2 position)
        {
            overridePosition = position;
        }

        public void OverrideZoom(float zoom)
        {
            overrideZoom = zoom;
        }

        public void ShakeCamera(float intensity, int duration)
        {
            shakeAmount.Add(intensity);
            shakeTimer.Add(duration);
        }
    }
}
