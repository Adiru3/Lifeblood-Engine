using System;
using Lifeblood.Engine;
using System.Collections.Generic;

namespace Lifeblood.Game
{
    public static class Combat
    {
        public class HitResult
        {
            public bool Hit;
            public Vector3 Point;
            public float Distance;
            public object Entity;
        }

        public static HitResult Raycast(Vector3 origin, Vector3 direction, List<Target> targets)
        {
            HitResult bestHit = new HitResult { Hit = false, Distance = float.MaxValue };

            foreach (var target in targets)
            {
                float t;
                if (RayIntersectsSphere(origin, direction, target.Position, target.Radius, out t))
                {
                    if (t < bestHit.Distance && t > 0)
                    {
                        bestHit.Hit = true;
                        bestHit.Distance = t;
                        bestHit.Point = origin + direction * t;
                        bestHit.Entity = target;
                    }
                }
            }
            // Can also raycast against players here if we add them to a list
            
            return bestHit;
        }

        // Ray-Sphere Intersection
        // Origin O, Direction D (normalized), Sphere Center C, Radius R
        // |X - C|^2 = R^2, X = O + tD
        // |O + tD - C|^2 = R^2
        // Let L = C - O
        // |tD - L|^2 = R^2
        // t^2 - 2t(D.L) + L.L - R^2 = 0
        public static bool RayIntersectsSphere(Vector3 origin, Vector3 direction, Vector3 center, float radius, out float distance)
        {
            distance = 0;
            Vector3 L = center - origin;
            float tca = Vector3.Dot(L, direction);
            
            if (tca < 0) return false; // Behind ray
            
            float d2 = Vector3.Dot(L, L) - tca * tca;
            float r2 = radius * radius;
            
            if (d2 > r2) return false;
            
            float thc = (float)Math.Sqrt(r2 - d2);
            distance = tca - thc;
            float t1 = tca + thc;
            
            if (distance < 0) distance = t1;
            if (distance < 0) return false;
            
            return true;
        }
    }

    public class Target
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Radius;
        public Vector3 BasePosition; // For movement patterns
        public float TimeOffset;
        public bool IsDead;
        public float RespawnTimer;
    }
}
