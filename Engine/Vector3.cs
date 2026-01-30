using System;

namespace Lifeblood.Engine
{
    public struct Vector3
    {
        public float X, Y, Z;

        public Vector3(float x, float y, float z)
        {
            X = x; Y = y; Z = z;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3 Normalize()
        {
            float len = Length();
            if (len > 0.0001f)
                return new Vector3(X / len, Y / len, Z / len);
            return new Vector3(0, 0, 0);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator *(Vector3 a, float scalar)
        {
            return new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        }

        public static Vector3 operator /(Vector3 a, float scalar)
        {
            return new Vector3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        }

        public static float Dot(Vector3 a, Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static Vector3 Zero { get { return new Vector3(0, 0, 0); } }
        public static Vector3 Up { get { return new Vector3(0, 1, 0); } }
        public static Vector3 Forward { get { return new Vector3(0, 0, 1); } }
        public static Vector3 Right { get { return new Vector3(1, 0, 0); } }

        public override string ToString()
        {
            return string.Format("({0:F2}, {1:F2}, {2:F2})", X, Y, Z);
        }
    }
}
