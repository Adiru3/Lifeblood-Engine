using System;

namespace Lifeblood.Engine
{
    /// <summary>
    /// Упрощенная 3D камера без зависимости от OpenTK
    /// </summary>
    public class Camera3D
    {
        public Vector3 Position;
        public float Pitch; // Вверх/вниз (радианы)
        public float Yaw;   // Влево/вправо (радианы)
        public float Roll;  // Наклон (обычно 0)

        public float FOV = 90.0f; // Градусы
        public float NearPlane = 0.1f;
        public float FarPlane = 1000.0f;

        public Camera3D(Vector3 position)
        {
            Position = position;
            Pitch = 0;
            Yaw = 0;
            Roll = 0;
        }

        public Vector3 GetForward()
        {
            float cosPitch = (float)Math.Cos(Pitch);
            return new Vector3(
                (float)Math.Sin(Yaw) * cosPitch,
                (float)Math.Sin(Pitch),
                (float)Math.Cos(Yaw) * cosPitch
            ).Normalize();
        }

        public Vector3 GetRight()
        {
            Vector3 forward = GetForward();
            return Vector3.Cross(forward, Vector3.Up).Normalize();
        }

        public Vector3 GetUp()
        {
            Vector3 forward = GetForward();
            Vector3 right = GetRight();
            return Vector3.Cross(right, forward).Normalize();
        }
    }
}
