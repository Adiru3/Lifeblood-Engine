using System;

namespace Lifeblood.Game
{
    /// <summary>
    /// 3D физика в стиле Quake World с bunny hopping, strafe jumping и air control
    /// </summary>
    public class Physics3D
    {
        // Константы Quake World
        public const float Gravity = 800.0f;
        public const float StopSpeed = 100.0f;
        public const float MaxVelocity = 320.0f;
        public const float JumpSpeed = 270.0f;
        
        // Ground Movement
        public const float GroundAccelerate = 10.0f;
        public const float Friction = 6.0f;
        
        // Air Movement (QW Style)
        public const float AirAccelFactor = 150.0f; // Increased for "Ideal" strafing
        public const float AirCap = 30.0f;
        
        // Player dimensions
        public const float PlayerHeight = 72.0f;
        public const float PlayerRadius = 32.0f;
        public const float EyeHeight = 64.0f;

        // Состояние
        public Engine.Vector3 Position;
        public Engine.Vector3 Velocity;
        public bool OnGround;
        private float groundCheckDistance = 2.0f;

        public Physics3D(Engine.Vector3 startPos)
        {
            Position = startPos;
            Velocity = Engine.Vector3.Zero;
            OnGround = false;
        }

        /// <summary>
        /// Применить трение (только на земле)
        /// </summary>
        public void ApplyFriction(float deltaTime)
        {
            if (!OnGround) return;

            float speed = Velocity.Length();
            if (speed < 0.1f)
            {
                Velocity = Engine.Vector3.Zero;
                return;
            }

            float drop = 0;
            float control = (speed < StopSpeed) ? StopSpeed : speed;
            drop = control * Friction * deltaTime;

            float newSpeed = speed - drop;
            if (newSpeed < 0) newSpeed = 0;
            
            if (speed > 0)
            {
                newSpeed /= speed;
                Velocity = Velocity * newSpeed;
            }
        }

        /// <summary>
        /// Ускорение на земле
        /// </summary>
        public void Accelerate(Engine.Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
        {
            float currentSpeed = Engine.Vector3.Dot(Velocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            
            if (addSpeed <= 0) return;

            float accelSpeed = accel * wishSpeed * deltaTime;
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            Velocity = Velocity + (wishDir * accelSpeed);
        }

        /// <summary>
        /// Ускорение в воздухе (Standard Quake)
        /// </summary>
        public void AirAccelerate(Engine.Vector3 wishDir, float wishSpeed, float accel, float deltaTime)
        {
            float wishSpd = wishSpeed;
            if (wishSpd > AirCap)
                wishSpd = AirCap;

            float currentSpeed = Engine.Vector3.Dot(Velocity, wishDir);
            float addSpeed = wishSpd - currentSpeed;

            if (addSpeed <= 0) return;

            float accelSpeed = accel * wishSpd * deltaTime; // Use Clamped WishSpeed (Quake logic)
            if (accelSpeed > addSpeed) accelSpeed = addSpeed;

            Velocity = Velocity + (wishDir * accelSpeed);
        }

        /// <summary>
        /// CPMA Style Air Control (Allows turning in air)
        /// </summary>
        public void AirControl(Engine.Vector3 wishDir, float wishSpeed, float deltaTime)
        {
            // Only apply air control if we are moving forward/side?
            // CPMA: If user presses W (Forward) and Turns mouse.
            
            float zSpeed = Math.Abs(Velocity.Y); 
            float speed = GetHorizontalSpeed();
            
            if (speed < 0.1f) return;
            
            // Dot product of velocity and wishdir
            float dot = Engine.Vector3.Dot(Velocity.Normalize(), wishDir);
            float k = 32.0f; // CPMA constant

            // Only apply if we are turning (dot < 0 implies very sharp turn, ignore? CPMA is usually dot > 0)
            if (dot > 0)
            {
                // We want to bend the velocity vector towards wishDir
                // But keep the speed constant (or increase slightly)
                
                // Original Quake just has AirAccelerate. 
                // CPMA adds a separate heuristic.
                
                float accel = AirAccelFactor * wishSpeed * deltaTime; // Reuse air accel
                
                Engine.Vector3 newVel = Velocity + wishDir * accel;
                newVel.Y = Velocity.Y; // Preserve gravity influence externally
                
                // Normalize to old speed (frictionless turning)
                newVel = newVel.Normalize() * speed;
                newVel.Y = Velocity.Y; // Restore Y
                
                Velocity = newVel;
            }
        }

        /// <summary>
        /// Применить гравитацию
        /// </summary>
        public void ApplyGravity(float deltaTime)
        {
            if (!OnGround)
            {
                Velocity.Y -= Gravity * deltaTime;
            }
        }

        /// <summary>
        /// Прыжок
        /// </summary>
        public void Jump()
        {
            if (OnGround)
            {
                Velocity.Y = JumpSpeed;
                OnGround = false;
            }
        }

        /// <summary>
        /// Обновить позицию
        /// </summary>
        public void UpdatePosition(float deltaTime)
        {
            Position = Position + (Velocity * deltaTime);
        }

        /// <summary>
        /// Отражение скорости от нормали (для коллизий)
        /// </summary>
        public void ClipVelocity(Engine.Vector3 normal, float overbounce = 1.0f)
        {
            float backoff = Engine.Vector3.Dot(Velocity, normal) * overbounce;
            
            if (backoff >= 0) return; // Движемся от стены

            Engine.Vector3 change = normal * backoff;
            Velocity = Velocity - change;
        }

        /// <summary>
        /// Проверка столкновения с землей (упрощенная)
        /// </summary>
        /// <summary>
        /// Проверка столкновения с землей (Ray/Box Cast down)
        /// </summary>
        public void CheckGround(System.Collections.Generic.List<Forms.Game3DWindow.Block> blocks)
        {
            // 1. Check Global Floor
            if (Position.Y <= 0.1f && Velocity.Y <= 0)
            {
                OnGround = true;
                Position.Y = 0;
                Velocity.Y = 0;
                return;
            }

            // 2. Check Blocks below
            if (blocks != null)
            {
                // Simple check: Is there a block directly below us?
                // Box cast down 2 units
                float minX = Position.X - 16;
                float maxX = Position.X + 16;
                float minZ = Position.Z - 16;
                float maxZ = Position.Z + 16;
                float testY = Position.Y - 2.0f; // Look down 2 units
                
                foreach(var b in blocks)
                {
                    float bMinX = b.X - 5; float bMaxX = b.X + 5;
                    float bMinY = b.Y - 5; float bMaxY = b.Y + 5;
                    float bMinZ = b.Z - 5; float bMaxZ = b.Z + 5;
                    
                    // Horizontal Overlap
                    if (maxX > bMinX && minX < bMaxX && maxZ > bMinZ && minZ < bMaxZ)
                    {
                        // Vertical Overlap with "Feet - 2" and Block Top
                        // We are "On" the block if our feet are near bMaxY
                        if (Position.Y >= bMaxY - 0.5f && testY < bMaxY)
                        {
                            OnGround = true;
                            Position.Y = bMaxY; // Snap to top
                            Velocity.Y = 0;
                            return;
                        }
                    }
                }
            }

            OnGround = false;
        }

        /// <summary>
        /// Update physics with collision against map blocks
        /// </summary>
        public void Update(Engine.Vector3 wishDir, bool jump, float deltaTime, System.Collections.Generic.List<Forms.Game3DWindow.Block> mapBlocks)
        {
            // 1. Проверка земли
            CheckGround(mapBlocks);

            // 2. Прыжок
            if (jump) // Check logic inside Jump (needs OnGround)
            {
                Jump();
            }

            // 3. Трение
            ApplyFriction(deltaTime);

            // 4. Ускорение
            if (OnGround)
            {
                Accelerate(wishDir, MaxVelocity, GroundAccelerate, deltaTime);
            }
            else
            {
                // Air Movement
                // 1. Standard Strafe (Side keys)
                AirAccelerate(wishDir, AirCap, AirAccelFactor, deltaTime);
                
                // 2. CPMA Air Control (Forward key + turn)
                if (GetHorizontalSpeed() > StopSpeed)
                {
                    AirControl(wishDir, 30.0f, deltaTime); 
                }
            }

            // 5. Gravity
            ApplyGravity(deltaTime);

            // 6. Move & Collide
            // X Axis
            Position.X += Velocity.X * deltaTime;
            if (CheckCollision(mapBlocks)) Position.X -= Velocity.X * deltaTime;

            // Z Axis
            Position.Z += Velocity.Z * deltaTime;
            if (CheckCollision(mapBlocks)) Position.Z -= Velocity.Z * deltaTime;
            
            // Y Axis
            Position.Y += Velocity.Y * deltaTime;
            
            // Note: CheckGround handles "Landing" state transition for next frame, 
            // but we must prevent falling through floor in this frame too?
            bool hitCeilingOrFloor = CheckCollision(mapBlocks);
            if (hitCeilingOrFloor)
            {
                 if (Velocity.Y < 0) // Landing on something
                 {
                     // Snap?
                     Position.Y -= Velocity.Y * deltaTime; // Backtrack
                     OnGround = true; // Landed
                     Velocity.Y = 0;
                 }
                 else if (Velocity.Y > 0) // Hit Ceiling
                 {
                     Position.Y -= Velocity.Y * deltaTime;
                     Velocity.Y = 0;
                 }
            }
            
            // Floor Check (Y=0 fallback)
            if (Position.Y < 0) { Position.Y = 0; Velocity.Y = 0; OnGround = true; }
        }
        
        private bool CheckCollision(System.Collections.Generic.List<Forms.Game3DWindow.Block> blocks)
        {
            // Player AABB
            // Width 32, Height 72
            float minX = Position.X - 16;
            float maxX = Position.X + 16;
            float minY = Position.Y;
            float maxY = Position.Y + 72;
            float minZ = Position.Z - 16;
            float maxZ = Position.Z + 16;
            
            if (blocks == null) return false;

            // Optimization: check only nearby blocks?
            // For now, iterate all (slow for large maps, ok for small arenas)
            foreach(var b in blocks)
            {
                // Block Size 10x10x10 (from Builder) - wait, renderer uses scale 10.
                float bMinX = b.X - 5; float bMaxX = b.X + 5;
                float bMinY = b.Y - 5; float bMaxY = b.Y + 5; // Center origin?
                float bMinZ = b.Z - 5; float bMaxZ = b.Z + 5;
                
                // Oops, Renderer3D renders at new Vector3(b.X, b.Y, b.Z) with scale 10.
                // If Cube is -0.5 to 0.5, then scale 10 means -5 to 5 relative to center.
                // So range is [X-5, X+5].
                
                if (maxX > bMinX && minX < bMaxX &&
                    maxY > bMinY && minY < bMaxY &&
                    maxZ > bMinZ && minZ < bMaxZ)
                {
                    return true;
                }
            }
            return false;
        }

        public float GetHorizontalSpeed()
        {
            return (float)Math.Sqrt(Velocity.X * Velocity.X + Velocity.Z * Velocity.Z);
        }

        public void ApplyImpulse(Engine.Vector3 force)
        {
            Velocity = Velocity + force * (1.0f / 1.0f); // mass = 1
            OnGround = false; // Launch off ground
        }

        /// <summary>
        /// Check for Wall Run possibility (Vector processing)
        /// </summary>
        public bool CanWallRun(Engine.Vector3 wallNormal)
        {
            // Vector processing: Dot product to determine possibility
            // If we are moving ALONG the wall (Dot ~ 0) or slightly INTO it?
            // Usually we want to run if we are parallel.
            // Wall Normal is perpendicular to wall.
            // Velocity should be perpendicular to Normal.
            float dot = Engine.Vector3.Dot(Velocity.Normalize(), wallNormal);
            
            // If dot is near 0, we are parallel.
            return Math.Abs(dot) < 0.3f && Velocity.Length() > 200;
        }
    }
}
