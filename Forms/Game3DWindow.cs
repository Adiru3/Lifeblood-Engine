using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using Lifeblood.Engine;
using System.Runtime.InteropServices;

namespace Lifeblood.Forms
{
    public class Game3DWindow : Form
    {
        private Engine.Camera3D camera;
        private Game.Physics3D physics;
        private Timer gameTimer;
        
        // Networking
        private Network.GameClient client;
        private bool isMultiplayer;
        
        // OpenGL Context
        private IntPtr hDC;
        private IntPtr hRC;
        
        private bool[] keys = new bool[256];
        private Point lastMousePos;
        private Game.Player localPlayer;
        private float gameTime = 0;
        
        // Aim Training State
        private List<Game.Target> targets = new List<Game.Target>();
        private bool aimTrainingMode = true; 
        private Game.WeaponType currentWeapon = Game.WeaponType.RocketLauncher;
        private float lastFireTime = 0;
        
        // Builder Mode State
        private bool builderMode = false;
        private int currentBlockType = 1; // 1=Wall, 2=Floor
        private List<Block> mapBlocks = new List<Block>();
        
        public struct Block
        {
            public int X, Y, Z;
            public int Type;
        }
        
        // Survival Mode
        public class Enemy
        {
            public Vector3 Position;
            public float Health = 100;
            public float Speed = 150.0f;
        }
        private List<Enemy> enemies = new List<Enemy>();
        private float spawnTimer = 0;
        private int score = 0;
        private int wave = 1;

        // Models & Resources
        // Models & Resources
        private Rendering.Renderer3D renderer;

        // Player Stats
        private Game.PlayerStats localStats = new Game.PlayerStats(Game.Settings.Nickname);

        public Game3DWindow(bool multiplayer = false)
        {
            this.isMultiplayer = multiplayer;
            if (isMultiplayer) aimTrainingMode = false;

            this.Text = "Lifeblood 3D - " + (isMultiplayer ? "MULTIPLAYER" : "AIM DOJO");
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Important for OpenGL in WinForms
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.Opaque, true);

            // Init Game Logic
            camera = new Engine.Camera3D(new Engine.Vector3(0, 70, -200));
            physics = new Game.Physics3D(new Engine.Vector3(0, 10, 0));
            localPlayer = new Game.Player(1, "LocalPlayer");
            localPlayer.Health = 100;

            if (isMultiplayer)
            {
                client = new Network.GameClient();
                client.Connect("127.0.0.1"); // Default to localhost
            }

            // Timer
            gameTimer = new Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
        }

        private void InitAimTraining()
        {
            targets.Clear();
            // Gridshot style (3x3 grid)
            for (int x = -1; x <= 1; x++)
            {
                for (int y = 0; y <= 2; y++)
                {
                    targets.Add(new Game.Target {
                        Position = new Vector3(x * 50, 50 + y * 40, 100),
                        BasePosition = new Vector3(x * 50, 50 + y * 40, 100),
                        Radius = 5.0f,
                        TimeOffset = (float)(new Random().NextDouble() * 10),
                        RespawnTimer = 0
                    });
                }
            }
            
            // Moving Targets (Player Models style)
            targets.Add(new Game.Target {
                Position = new Vector3(100, 72, 50),
                BasePosition = new Vector3(100, 72, 50),
                Radius = 16.0f, // Player Width
                Velocity = new Vector3(0, 0, 1), // Z movement
                TimeOffset = 0
            });
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            InitOpenGL();
            InitResources();
            if (aimTrainingMode) InitAimTraining();
            
            gameTimer.Start();
            
            // Hide Cursor
            Cursor.Hide();
            lastMousePos = new Point(Width / 2, Height / 2);
            Cursor.Position = new Point(this.Left + Width / 2, this.Top + Height / 2);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            gameTimer.Stop();
            if (hRC != IntPtr.Zero)
            {
                WGL.wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);
                WGL.wglDeleteContext(hRC);
            }
            if (client != null) client.Disconnect();
            base.OnHandleDestroyed(e);
        }

        private void InitOpenGL()
        {
            hDC = GetDC(this.Handle);

            WGL.PIXELFORMATDESCRIPTOR pfd = new WGL.PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)Marshal.SizeOf(typeof(WGL.PIXELFORMATDESCRIPTOR));
            pfd.nVersion = 1;
            pfd.dwFlags = WGL.PFD_DRAW_TO_WINDOW | WGL.PFD_SUPPORT_OPENGL | WGL.PFD_DOUBLEBUFFER;
            pfd.iPixelType = WGL.PFD_TYPE_RGBA;
            pfd.cColorBits = 32;
            pfd.cDepthBits = 24;
            pfd.iLayerType = WGL.PFD_MAIN_PLANE;

            int format = WGL.ChoosePixelFormat(hDC, ref pfd);
            WGL.SetPixelFormat(hDC, format, ref pfd);

            hRC = WGL.wglCreateContext(hDC);
            WGL.wglMakeCurrent(hDC, hRC);

            // Basic GL Setup
            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glEnable(GL.GL_CULL_FACE);
            GL.glClearColor(0.1f, 0.1f, 0.15f, 1.0f);
            
            // Setup Projection
            ResizeGL();
        }

        private void InitResources()
        {
            Lifeblood.Modding.ModLoader.Initialize(); // Load Mods
            Lifeblood.Network.DownloadManager.Initialize();
            
            // Sim: Check for a "server mod" map
            if (isMultiplayer)
            {
               Lifeblood.Network.DownloadManager.CheckAndDownload("server_map.txt", "http://server");
            }

            renderer = new Rendering.Renderer3D();
            
            if (!isMultiplayer)
            {
                InitArena();
            }
        }

        private void InitArena()
        {
            mapBlocks.Clear();
            
            // Floor (100x100)
            // Center is 0,0. Range -500 to 500? No, block units are intervals of 10.
            // Let's make an arena of size 50x50 blocks (500x500 units) centered at 0.
            int size = 40; // 400x400 units
            for (int x = -size; x <= size; x++)
            {
                for (int z = -size; z <= size; z++)
                {
                    mapBlocks.Add(new Block { X = x * 10, Y = 0, Z = z * 10, Type = 2 }); // Floor
                }
            }
            
            // Walls
            for(int x = -size; x <= size; x++)
            {
                for(int h=1; h<=5; h++) // Height 5
                {
                    mapBlocks.Add(new Block { X = x * 10, Y = h*10, Z = -size * 10, Type = 1 }); // North Wall
                    mapBlocks.Add(new Block { X = x * 10, Y = h*10, Z = size * 10, Type = 1 });  // South Wall
                }
            }
             for(int z = -size; z <= size; z++)
            {
                for(int h=1; h<=5; h++) // Height 5
                {
                    mapBlocks.Add(new Block { X = -size * 10, Y = h*10, Z = z * 10, Type = 1 }); // West Wall
                    mapBlocks.Add(new Block { X = size * 10, Y = h*10, Z = z * 10, Type = 1 });  // East Wall
                }
            }
            
            // Random Pillars/Cover
            Random r = new Random(12345); // Fixed seed for consistent arena
            for(int i=0; i<30; i++)
            {
                int px = r.Next(-size+5, size-5) * 10;
                int pz = r.Next(-size+5, size-5) * 10;
                // Pillar 2x2
                for (int h=1; h<=4; h++)
                {
                     mapBlocks.Add(new Block { X = px, Y = h*10, Z = pz, Type = 1 });
                     mapBlocks.Add(new Block { X = px+10, Y = h*10, Z = pz, Type = 1 });
                     mapBlocks.Add(new Block { X = px, Y = h*10, Z = pz+10, Type = 1 });
                     mapBlocks.Add(new Block { X = px+10, Y = h*10, Z = pz+10, Type = 1 });
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (hRC != IntPtr.Zero) ResizeGL();
        }

        private void ResizeGL()
        {
            GL.glViewport(0, 0, Width, Height);
            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glLoadIdentity();
            GL.gluPerspective(Game.Settings.FOV, (double)Width / Height, 0.1, 1000.0);
            GL.glMatrixMode(GL.GL_MODELVIEW);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            float deltaTime = 0.016f;
            gameTime += deltaTime;
            
            // Mode Toggling
            if (keys[(int)Keys.F1]) { builderMode = !builderMode; keys[(int)Keys.F1] = false; }

            HandleInput(deltaTime);
            UpdatePhysics(deltaTime); // Use noclip if builder?
            
            if (builderMode) 
            {
                UpdateBuilder(deltaTime);
            }
            else if (aimTrainingMode) 
            {
                // Aim Training Replaced by Survival Mode
            }
            
            Render();
        }

        private void UpdateSurvival(float dt)
        {
            // Game Over Check
            if (localPlayer.Health <= 0)
            {
                // Reset Game
                localPlayer.Health = 100;
                score = 0;
                wave = 1;
                localStats = new Game.PlayerStats(Game.Settings.Nickname); // Reset Stats
                localStats.Deaths++; // Count the death
                enemies.Clear();
                physics.Position = new Vector3(0, 50, 0); // Reset Spawn
                return;
            }

            // Spawning
            spawnTimer -= dt;
            if (spawnTimer <= 0)
            {
                spawnTimer = 3.0f - (wave * 0.1f); // Faster spawns per wave
                if (spawnTimer < 0.5f) spawnTimer = 0.5f;
                
                // Spawn random enemy within Arena INT Bounds (-400 to 400)
                // Keep inside walls (range -380 to 380)
                Random r = new Random();
                float x = r.Next(-380, 380);
                float z = r.Next(-380, 380);
                
                Vector3 spawnPos = new Vector3(x, 20, z);
                enemies.Add(new Enemy { Position = spawnPos, Health = 100 });
            }
            
            // Update Enemies
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var e = enemies[i];
                // Chase Player
                Vector3 dir = (physics.Position - e.Position).Normalize();
                Vector3 newPos = e.Position + dir * e.Speed * dt;
                
                // Simple Enemy Collision with Map
                // Check if newPos is inside a wall block
                bool collided = false;
                // Optimization: Just check simplistic grid based? 
                // Let's reuse a simple check logic similar to Physics3D but local here
               
                foreach(var b in mapBlocks)
                {
                    if (b.Type == 2) continue; // Ignore floor
                    float bMinX = b.X - 6; float bMaxX = b.X + 6; // slightly larger than block radius 5
                    float bMinZ = b.Z - 6; float bMaxZ = b.Z + 6;
                    
                    if (newPos.X > bMinX && newPos.X < bMaxX && newPos.Z > bMinZ && newPos.Z < bMaxZ)
                    {
                        collided = true;
                        break;
                    }
                }
                
                if (!collided)
                {
                     e.Position = newPos;
                }
                
                e.Position.Y = 20 + (float)Math.Sin(gameTime * 5 + i) * 5; // Bobbing
                
                // Collision with player (Damage)
                if ((physics.Position - e.Position).Length() < 30)
                {
                    localPlayer.Health -= 1;
                    // Push back
                    physics.Velocity = physics.Velocity + dir * 500;
                }
                
                if (e.Health <= 0)
                {
                    enemies.RemoveAt(i);
                    score += 100;
                    localStats.Kills++;
                    
                    // Simulate Headshot (20% chance)
                    if (new Random().NextDouble() < 0.2) localStats.Headshots++;
                    
                    if (score % 1000 == 0) wave++;
                }
            }

            // Shooting
            bool fire = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (fire && gameTime - lastFireTime > Game.WeaponDef.Get(currentWeapon).FireRate)
            {
                lastFireTime = gameTime;
                localStats.ShotsFired++;
                
                // Calculate Ray
                Vector3 dir = new Vector3(
                    (float)Math.Sin(camera.Yaw) * (float)Math.Cos(camera.Pitch),
                    (float)Math.Sin(camera.Pitch),
                    (float)Math.Cos(camera.Yaw) * (float)Math.Cos(camera.Pitch)
                ).Normalize();
                
                // Simple Sphere Intersection for Enemies
                bool hitAnything = false;
                foreach(var e in enemies)
                {
                    float dist;
                    if (Game.Combat.RayIntersectsSphere(camera.Position, dir, e.Position, 20, out dist)) 
                    {
                        int dmg = Game.WeaponDef.Get(currentWeapon).Damage;
                        e.Health -= dmg;
                        
                        localStats.ShotsHit++;
                        localStats.DamageDealt += dmg;
                        hitAnything = true;
                        
                        // Check Kill
                        if (e.Health <= 0) 
                        {
                            // Already handled in loop? No, damage application here might kill.
                            // But usually we check e.Health <= 0 in the Update loop later.
                            // Let's add Kill credit here or in the cleanup loop?
                            // Cleanup loop handles removal and Score logic.
                            // We'll update Kills there.
                        }
                    }
                }
                
                // Rocket Jump Logic (if using RL)
                if (currentWeapon == Game.WeaponType.RocketLauncher)
                {
                    // Raycast to world (map blocks or floor)
                    // Simplified: Assume Ray hits floor if pointing down?
                    // Better: Implement Ray-Map intersection.
                    // For now: Quick hack for Rocket Jumping off floor.
                    if (camera.Pitch < -0.5f) // Looking down
                    {
                        // Assume hit ground near feet
                        Vector3 explosionPos = physics.Position + dir * 50; // 50 units away
                        float dist = (physics.Position - explosionPos).Length();
                        
                        if (dist < 100)
                        {
                            // Calculate Impulse
                            Vector3 kbDir = (physics.Position - explosionPos).Normalize();
                            float force = 800.0f * (1.0f - dist / 100.0f); // 800 force max
                            physics.ApplyImpulse(kbDir * force + new Vector3(0, 400, 0)); // Add upward bias
                        }
                    }
                }
            }
            
            // Weapon Switch
            if (keys[(int)Keys.D1]) currentWeapon = Game.WeaponType.Knife;
            if (keys[(int)Keys.D2]) currentWeapon = Game.WeaponType.Pistol;
            if (keys[(int)Keys.D3]) currentWeapon = Game.WeaponType.Shotgun;
            if (keys[(int)Keys.D4]) currentWeapon = Game.WeaponType.Assault; // Was MG
            if (keys[(int)Keys.D5]) currentWeapon = Game.WeaponType.Scout;   // Was Railgun
            if (keys[(int)Keys.D6]) currentWeapon = Game.WeaponType.RocketLauncher;
            if (keys[(int)Keys.D7]) currentWeapon = Game.WeaponType.Deagle;  // Replaced Shaft with Deagle
        }

        private void HandleInput(float deltaTime)
        {
            if (this.Focused)
            {
                // Mouse Look
                Point center = PointToScreen(new Point(Width / 2, Height / 2));
                Point currentMousePos = Cursor.Position;
                
                int dx = currentMousePos.X - center.X;
                int dy = currentMousePos.Y - center.Y;

                if (dx != 0 || dy != 0)
                {
                    float sensitivity = Game.Settings.MouseSensitivity * 0.001f;
                    camera.Yaw -= dx * sensitivity;
                    camera.Pitch -= dy * sensitivity;

                    float maxPitch = (float)Math.PI / 2 - 0.01f;
                    if (camera.Pitch > maxPitch) camera.Pitch = maxPitch;
                    if (camera.Pitch < -maxPitch) camera.Pitch = -maxPitch;

                    Cursor.Position = center;
                }
            }
        }

        private void UpdatePhysics(float deltaTime)
        {
            float forward = 0;
            float right = 0;
            bool jump = false;

            if (keys[(int)Game.Settings.GetBind("Forward")]) forward += 1;
            if (keys[(int)Game.Settings.GetBind("Back")]) forward -= 1;
            
            // Swap logic: if D (Right) is pressed, we want +1 Right. 
            // My previous analysis said D currently moves Left (+X vs -Z camera).
            // Let's just INVERT the Right vector contribution in wishDir calculation line 306 instead?
            // Or simpler: Swapping key logic here.
            // If "Right" key is pressed -> right -= 1 (Move Left in logic, which is Physical Right?)
            // Let's negate strictly.
            
            if (keys[(int)Game.Settings.GetBind("Right")]) right -= 1; // Inverted based on user report: "D goes Left" -> Now D subtracts? 
            if (keys[(int)Game.Settings.GetBind("Left")]) right += 1;  // A adds?
            
            // Wait, previous code was:
            // if (Right) right += 1; -> User said "D goes Left".
            // So if I want D to go Right, I should Invert the RESULT of the vector or the input.
            // If +1 resulted in Left, then -1 should result in Right.
            // So Right Key should be -1. Left Key should be +1.
            
            if (keys[(int)Game.Settings.GetBind("Jump")]) jump = true;

            Engine.Vector3 wishDir = Engine.Vector3.Zero;
            if (Math.Abs(forward) > 0.01f || Math.Abs(right) > 0.01f)
            {
                // Standard FPS Rotation
                // If Yaw=0 (Forward=Z?), X=Right.
                // Formula: X = Sin(Yaw)*F + Cos(Yaw)*R
                //          Z = Cos(Yaw)*F - Sin(Yaw)*R
                // If Yaw=0: X = R, Z = F.
                // If R=-1 (Right Key), X=-1 (Left). 
                // So "Right Key -> -1" creates Left Movement.
                // User said "D (Right Key) goes Left". 
                // My previous code was: Right Key -> +1. 
                // So +1 produced Left movement? That implies X axis is inverted or Camera is inverted.
                // To fix "D goes Right":
                // If +1 -> Left, then -1 -> Right.
                // So: Right Key -> -1. Left Key -> +1.
                
                wishDir = new Engine.Vector3(
                    (float)Math.Sin(camera.Yaw) * forward + (float)Math.Cos(camera.Yaw) * right,
                    0,
                    (float)Math.Cos(camera.Yaw) * forward - (float)Math.Sin(camera.Yaw) * right
                );
                wishDir = wishDir.Normalize();
            }

            physics.Update(wishDir, jump, deltaTime, isMultiplayer ? null : mapBlocks);
            camera.Position = physics.Position + new Engine.Vector3(0, Game.Physics3D.EyeHeight, 0);
            
            // Survival Mode Logic (if not builder and not MP)
            if (!builderMode && !isMultiplayer)
            {
                UpdateSurvival(deltaTime);
            }
            
            if (isMultiplayer && client != null)
            {
                var input = new Network.PlayerInputPacket
                {
                    Sequence = (uint)gameTime,
                    DeltaTime = deltaTime,
                    Forward = forward,
                    Right = right,
                    Jump = jump,
                    Yaw = camera.Yaw,
                    Pitch = camera.Pitch
                };
                client.SendInput(input);
            }
        }

        private void Render()
        {
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            GL.glLoadIdentity();

            // Apply Camera
            Engine.Vector3 target = camera.Position + new Engine.Vector3(
                (float)Math.Sin(camera.Yaw) * (float)Math.Cos(camera.Pitch),
                (float)Math.Sin(camera.Pitch),
                (float)Math.Cos(camera.Yaw) * (float)Math.Cos(camera.Pitch)
            );
            
            GL.gluLookAt(camera.Position.X, camera.Position.Y, camera.Position.Z,
                         target.X, target.Y, target.Z,
                         0, 1, 0);

            // Draw Scene
            renderer.RenderScene();
            
            // Draw Map Blocks (Arena or Custom)
            if (mapBlocks.Count > 0)
            {
                 foreach(var b in mapBlocks)
                 {
                     float r=0.5f, g=0.5f, bl=0.5f;
                     if(b.Type == 1) { r=0.8f; g=0.8f; bl=0.8f; } // Wall
                     if(b.Type == 2) { r=0.4f; g=0.4f; bl=0.5f; } // Floor
                     renderer.RenderMesh(renderer.cubeMesh, new Vector3(b.X, b.Y, b.Z), 10, r, g, bl);
                 }
            }

            if (builderMode)
            {
                RenderBuilder(); // Only Ghost and HUD
            }
            else if (!isMultiplayer) 
            {
                // Render Survival Enemies
                foreach(var e in enemies)
                {
                    // Simple Red Box for Enemy
                    renderer.RenderMesh(renderer.cubeMesh, new Vector3(e.Position.X, e.Position.Y, e.Position.Z), 20, 1, 0, 0);
                }
            }
            
            // Draw Players
            if (isMultiplayer && client != null)
            {
                lock (client.OtherPlayers)
                {
                    foreach (var p in client.OtherPlayers.Values)
                    {
                        renderer.RenderPlayer(p);
                    }
                }
            }
            
            // Render Weapon (Viewmodel)
            // Clear Depth Buffer slightly or just draw on top? 
            // Better to clear depth if we want it perfect, but standard approach:
            GL.glClear(GL.GL_DEPTH_BUFFER_BIT); // Clear depth so gun is always on top of world
            
            float fireAnim = 0;
            if (gameTime - lastFireTime < 0.2f)
            {
               fireAnim = 1.0f - (gameTime - lastFireTime) / 0.2f;
            }
            renderer.RenderViewModel(currentWeapon, fireAnim);

            RenderHUD();

            WGL.SwapBuffers(hDC);
        }
        
        private void UpdateBuilder(float dt)
        {
            // Block Selection
            if (keys[(int)Keys.D1]) currentBlockType = 1; // Wall
            if (keys[(int)Keys.D2]) currentBlockType = 2; // Floor/Detail

            // Raycast for Block Placement
            // Simple Raycast against Plane Y=0 initially, or existing blocks?
            // Let's implement a simple "Reach" raycast.
            
            // Camera Direction
            Vector3 dir = new Vector3(
                (float)Math.Sin(camera.Yaw) * (float)Math.Cos(camera.Pitch),
                (float)Math.Sin(camera.Pitch),
                (float)Math.Cos(camera.Yaw) * (float)Math.Cos(camera.Pitch)
            ).Normalize();
            
            // Parametric Ray: P = O + D * t
            // Intersect with ground plane (Y=0) or Y=50 etc.
            // Simplified: Just place in front of player at distance 5, snapped to grid.
            
            Vector3 targetPos = camera.Position + dir * 10; // 10 units away
            int bx = (int)Math.Round(targetPos.X / 10.0f) * 10;
            int by = (int)Math.Round(targetPos.Y / 10.0f) * 10;
            int bz = (int)Math.Round(targetPos.Z / 10.0f) * 10;
            
            // Limit lowest Y
            if (by < 0) by = 0;

            // Save/Load
            if (keys[(int)Keys.P])
            {
                // Save Map
                try
                {
                    using (System.IO.StreamWriter w = new System.IO.StreamWriter("custom_map.txt"))
                    {
                        foreach (var b in mapBlocks) w.WriteLine(string.Format("{0},{1},{2},{3}", b.X, b.Y, b.Z, b.Type));
                    }
                    Console.WriteLine("Map Saved!");
                }
                catch { }
                keys[(int)Keys.P] = false;
            }
            if (keys[(int)Keys.L])
            {
                // Load Map
                try
                {
                   if (System.IO.File.Exists("custom_map.txt"))
                   {
                       mapBlocks.Clear();
                       foreach(string line in System.IO.File.ReadAllLines("custom_map.txt"))
                       {
                           var p = line.Split(',');
                           mapBlocks.Add(new Block { X=int.Parse(p[0]), Y=int.Parse(p[1]), Z=int.Parse(p[2]), Type=int.Parse(p[3]) });
                       }
                       Console.WriteLine("Map Loaded!");
                   }
                }
                catch { }
                keys[(int)Keys.L] = false;
            }

            // Handle Clicks (Debounce needed or use MouseDown event)
            if ((Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left)
            {
               // Add Block if not exists
               if (!mapBlocks.Exists(b => b.X == bx && b.Y == by && b.Z == bz))
               {
                   mapBlocks.Add(new Block { X=bx, Y=by, Z=bz, Type=currentBlockType });
                   System.Threading.Thread.Sleep(100); // Simple debounce
               }
            }
            if ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right)
            {
               // Remove Block
               mapBlocks.RemoveAll(b => b.X == bx && b.Y == by && b.Z == bz);
               System.Threading.Thread.Sleep(100);
            }
        }
        
        private void RenderBuilder()
        {
            // Placed blocks rendered in main loop now
            
            // Render Ghost Block (at cursor)
             Vector3 dir = new Vector3(
                (float)Math.Sin(camera.Yaw) * (float)Math.Cos(camera.Pitch),
                (float)Math.Sin(camera.Pitch),
                (float)Math.Cos(camera.Yaw) * (float)Math.Cos(camera.Pitch)
            ).Normalize();
            Vector3 targetPos = camera.Position + dir * 10;
            int bx = (int)Math.Round(targetPos.X / 10.0f) * 10;
            int by = (int)Math.Round(targetPos.Y / 10.0f) * 10;
            int bz = (int)Math.Round(targetPos.Z / 10.0f) * 10;
            if (by < 0) by = 0;
            
            renderer.RenderMesh(renderer.cubeMesh, new Vector3(bx, by, bz), 10, 0, 1, 0); // Green Ghost
            
            // HUD Info
             // Switch to 2D Orthographic Projection
            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glPushMatrix();
            GL.glLoadIdentity();
            GL.glOrtho(0, Width, Height, 0, -1, 1);
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();
            GL.glDisable(GL.GL_DEPTH_TEST);
            
            // Draw Builder UI?
            // Just a text indicator (using rects as we have no font)
            // Left Top: Builder Mode
            DrawRect(10, 10, 20, 20, 1, 1, 0); 
            
            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glPopMatrix();
            GL.glMatrixMode(GL.GL_MODELVIEW);
        }

        private void RenderHUD()
        {
            // Switch to 2D Orthographic Projection
            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glPushMatrix();
            GL.glLoadIdentity();
            GL.glViewport(0, 0, Width, Height); // Ensure viewport match
            // glOrtho(left, right, bottom, top, near, far)
            // Map 0..Width, Height..0 (Top-Left origin like WinForms/GDI)
            // Or Bottom-Left origin standard GL? Let's use 0..Width, 0..Height (Bottom-Left)
            // Or let's match window pixels: 0, w, h, 0 -> Top-Left origin
             GL.glOrtho(0, Width, Height, 0, -1, 1);
            
            GL.glMatrixMode(GL.GL_MODELVIEW);
            GL.glLoadIdentity();
            
            GL.glDisable(GL.GL_DEPTH_TEST);
            GL.glDisable(GL.GL_CULL_FACE);
            GL.glDisable(GL.GL_TEXTURE_2D);

            // Crosshair
            int cx = Width / 2;
            int cy = Height / 2;
            int size = 10;
            
            GL.glBegin(GL.GL_LINES);
            GL.glColor3f(0, 1, 0); // Green
            GL.glVertex3f(cx - size, cy, 0); GL.glVertex3f(cx + size, cy, 0);
            GL.glVertex3f(cx, cy - size, 0); GL.glVertex3f(cx, cy + size, 0);
            GL.glEnd();
            
            // Health Bar (Bottom Left)
            // Simple Quads for text/bar placeholders
            // Since we don't have a Font Renderer in pure GL 1.1 easily (wglUseFontBitmaps requires setup), 
            // we will draw bars.
            
            // Health BG
            DrawRect(20, Height - 40, 200, 20, 0.2f, 0.0f, 0.0f);
            // Health FG
            float hpPct = localPlayer.Health / 100.0f;
            DrawRect(20, Height - 40, (int)(200 * hpPct), 20, 1.0f, 0.0f, 0.0f);
            
            // Weapon Info (Bottom Right)
            // Just a color code for weapon for now
            DrawRect(Width - 120, Height - 40, 100, 20, 0.5f, 0.5f, 0.5f);
            
            // Restore 3D
            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glEnable(GL.GL_CULL_FACE);
            
            GL.glMatrixMode(GL.GL_PROJECTION);
            GL.glPopMatrix();
            GL.glMatrixMode(GL.GL_MODELVIEW);
        }

        private void DrawRect(int x, int y, int w, int h, float r, float g, float b)
        {
            GL.glColor3f(r, g, b);
            GL.glBegin(GL.GL_QUADS);
            GL.glVertex3f(x, y, 0);
            GL.glVertex3f(x + w, y, 0);
            GL.glVertex3f(x + w, y + h, 0);
            GL.glVertex3f(x, y + h, 0);
            GL.glEnd();
        }
        
        private void RenderAimTraining()
        {
            foreach (var t in targets)
            {
                if (t.RespawnTimer <= 0)
                {
                    if (t.Velocity.Length() > 0)
                    {
                         // Render as Player
                         var p = new Network.PlayerStatePacket {
                             PosX = t.Position.X, PosY = t.Position.Y, PosZ = t.Position.Z,
                             Yaw = 0 // Face forward
                         };
                         renderer.RenderPlayer(p);
                    }
                    else
                    {
                        // Render as Sphere
                        renderer.RenderSphere(t.Position, t.Radius, 0, 1, 1);
                    }
                }
            }
            
            // Draw Crosshair (in world space? No, need 2D overlay)
            // Or simple 3D lines near camera
            // Using small crosshair at distance
            Vector3 camDir = new Vector3(
                (float)Math.Sin(camera.Yaw) * (float)Math.Cos(camera.Pitch),
                (float)Math.Sin(camera.Pitch),
                (float)Math.Cos(camera.Yaw) * (float)Math.Cos(camera.Pitch)
            ).Normalize();
            
            Vector3 chPos = camera.Position + camDir * 10;
            // renderer.RenderSphere(chPos, 0.05f, 0, 1, 0); // Dot crosshair
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Close();
            keys[(int)e.KeyCode] = true;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            keys[(int)e.KeyCode] = false;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
    }
}
