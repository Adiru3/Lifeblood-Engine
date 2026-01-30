using System;
using System.Collections.Generic;

namespace Lifeblood.Game
{
    public class Physics
    {
        // Quake World Constants
        public const float StopSpeed = 100.0f;
        public const float DuckScale = 0.25f;
        public const float Gravity = 800.0f;
        public const float AccelRate = 10.0f; 
        public const float AirAccelRate = 150.0f; // High for strafing (CPMA style)
        public const float Friction = 6.0f;
        public const float MaxVelocity = 320.0f; 
        public const float MaxAirVelocity = 30.0f; // Soft cap for air strafing gain

        // State
        public Vector2 Velocity;
        public Vector2 Position;
        public bool OnGround;

        public Physics(Vector2 startPos)
        {
            Position = startPos;
            Velocity = new Vector2(0, 0);
            OnGround = true; // Simplified for 2.5D
        }

        public void ApplyFriction(float t)
        {
            Vector2 vec = Velocity;
            float speed = vec.Length();
            float drop = 0;
            float control = (speed < StopSpeed) ? StopSpeed : speed;

            // Only apply friction on ground
            if (OnGround)
            {
                drop += control * Friction * t;
            }

            float newSpeed = speed - drop;
            if (newSpeed < 0) newSpeed = 0;
            
            if (speed > 0)
            {
                newSpeed /= speed;
                Velocity = Velocity * newSpeed;
            }
        }

        public void Accelerate(Vector2 wishDir, float wishSpeed, float accel, float t)
        {
            float currentSpeed = Vector2.Dot(Velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            
            if (addSpeed <= 0) return;

            float accelSpeed = accel * wishSpeed * t;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            Velocity = Velocity + (wishDir * accelSpeed);
        }

        public void AirAccelerate(Vector2 wishDir, float wishSpeed, float accel, float t)
        {
            float wishSpd = wishSpeed;
            if (wishSpd > MaxAirVelocity) wishSpd = MaxAirVelocity; 
            
            float currentSpeed = Vector2.Dot(Velocity, wishDir);
            float addSpeed = wishSpd - currentSpeed;

            if (addSpeed <= 0) return;

            float accelSpeed = accel * wishSpeed * t;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            Velocity = Velocity + (wishDir * accelSpeed);
        }

        public void Step(float t)
        {
            Position = Position + (Velocity * t);
        }
        
        // Wall Sliding
        public void ClipVelocity(Vector2 normal, float overbounce)
        {
            float backoff = Vector2.Dot(Velocity, normal) * overbounce;
            
            if (backoff >= 0) return; // Moving away from wall

            Vector2 change = normal * backoff;
            Velocity = Velocity - change;
        }
    }
}
