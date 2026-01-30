using System;
using System.Drawing;
using System.Windows.Forms;
using Lifeblood.Rendering;
using Lifeblood.Game;
using System.Collections.Generic;

namespace Lifeblood.Forms
{
    public class GameWindow : Form
    {
        private Timer gameLoop;
        private Renderer3D renderer;
        private Game.Physics physics;
        private float playerAngle = 0;
        private float walkTime = 0; // For bobbing
        
        private Player localPlayer;
        private List<Player> bots;
        private Label uiLabel;
        private Label weaponMenuLabel;
        private bool showBuyMenu = false;
        
        private bool[] keys = new bool[256];
        private float matchTimer = 600.0f; 
        
        // Recoil
        private float recoilPitch = 0;
        private Random rng = new Random();

        public GameWindow(string mapPath = null)
        {
            this.Text = mapPath == null ? "Lifeblood - Aim Dojo" : "Lifeblood - Custom Map: " + System.IO.Path.GetFileName(mapPath);
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Cursor = new Cursor(Cursor.Current.Handle);
            
            Game.Vector2 startPos = new Game.Vector2(0, -100);
            physics = new Game.Physics(startPos);

            localPlayer = new Player(1, "Player");
            localPlayer.Buy("ak47");
            localPlayer.Buy("helmet");

            bots = new List<Player>();
            SpawnBots();

            uiLabel = new Label();
            uiLabel.AutoSize = true;
            uiLabel.ForeColor = Color.Yellow;
            uiLabel.BackColor = Color.Transparent;
            uiLabel.Font = new Font("Consolas", 16, FontStyle.Bold);
            uiLabel.Location = new Point(20, this.Height - 80);
            this.Controls.Add(uiLabel);
            
            // ... (Menu setup omitted for brevity in tool input as it overlaps with existing code, but I need to make sure I don't delete it. 
            // In ReplaceFileContent, I must include all content between Start/End or effectively rewrite the block.
            // I will assume the tool replaces perfectly. 
            // The existing code has `weaponMenuLabel` setup. I'll include it.
            
            weaponMenuLabel = new Label();
            weaponMenuLabel.AutoSize = true;
            weaponMenuLabel.ForeColor = Color.White;
            weaponMenuLabel.BackColor = Color.FromArgb(100, 0, 0, 0); 
            weaponMenuLabel.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            weaponMenuLabel.Text = "[B] WEAPON MENU\n1. AK-47\n2. Deagle\n3. Scout (Sniper)\n4. Shotgun\n5. Knife";
            weaponMenuLabel.Location = new Point(20, 100);
            weaponMenuLabel.Visible = false;
            this.Controls.Add(weaponMenuLabel);

            renderer = new Renderer3D();
            
            if (mapPath != null && System.IO.File.Exists(mapPath))
            {
                LoadMapFromFile(mapPath);
            }
            else
            {
                BuildDojoMap();
            }

            gameLoop = new Timer();
            gameLoop.Interval = 10;
            gameLoop.Tick += Update;
            gameLoop.Start();

            this.KeyDown += OnKeyDown;
            this.KeyUp += (s, e) => keys[e.KeyValue] = false;
        }

        private void LoadMapFromFile(string path)
        {
            renderer.MapWalls.Clear();
            try {
                string[] lines = System.IO.File.ReadAllLines(path);
                foreach(string line in lines) {
                    string[] p = line.Split(',');
                    if(p.Length >= 7) {
                        renderer.MapWalls.Add(new Renderer3D.Wall {
                            X1 = float.Parse(p[0]), Y1 = float.Parse(p[1]),
                            X2 = float.Parse(p[2]), Y2 = float.Parse(p[3]),
                            Color = Color.FromArgb(int.Parse(p[4]), int.Parse(p[5]), int.Parse(p[6]))
                        });
                    }
                }
            } catch { MessageBox.Show("Failed to load map: " + path); }
        }

        private void SpawnBots()
        {
            bots.Clear();
            for(int i=0; i<8; i++) bots.Add(new Player(10+i, "Bot") { Health=100 });
        }

        private void BuildDojoMap()
        {
            renderer.MapWalls.Clear();
            
            // "Aim Dojo" - Large brightly lit training hall
            // Main Room (Huge)
            int sz = 400;
            Color wallCol = Color.DarkGray; // Lit walls
            
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = -sz, Y1 = -sz, X2 = sz, Y2 = -sz, Color = wallCol });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = sz, Y1 = -sz, X2 = sz, Y2 = sz, Color = wallCol });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = sz, Y1 = sz, X2 = -sz, Y2 = sz, Color = wallCol });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = -sz, Y1 = sz, X2 = -sz, Y2 = -sz, Color = wallCol });
            
            // Training Lanes (Barriers)
            Color barrierCol = Color.Crimson;
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = -100, Y1 = 50, X2 = 100, Y2 = 50, Color = barrierCol });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = -100, Y1 = 150, X2 = 100, Y2 = 150, Color = barrierCol });
            
            // Pillars for peeking
            AddPillar(200, 0, 30, Color.Orange);
            AddPillar(-200, 0, 30, Color.Orange);
        }

        private void AddPillar(int x, int y, int size, Color c)
        {
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = x, Y1 = y, X2 = x+size, Y2 = y, Color = c });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = x+size, Y1 = y, X2 = x+size, Y2 = y+size, Color = c });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = x+size, Y1 = y+size, X2 = x, Y2 = y+size, Color = c });
            renderer.MapWalls.Add(new Renderer3D.Wall { X1 = x, Y1 = y+size, X2 = x, Y2 = y, Color = c });
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
             keys[e.KeyValue] = true;
             
             if (e.KeyCode == Keys.B) {
                 showBuyMenu = !showBuyMenu;
                 weaponMenuLabel.Visible = showBuyMenu;
             }
             
             if (showBuyMenu) {
                 if (e.KeyCode == Keys.D1) { localPlayer.PrimaryWeapon = WeaponType.AK47; showBuyMenu=false; }
                 if (e.KeyCode == Keys.D2) { localPlayer.PrimaryWeapon = WeaponType.Deagle; showBuyMenu=false; }
                 if (e.KeyCode == Keys.D3) { localPlayer.PrimaryWeapon = WeaponType.Scout; showBuyMenu=false; }
                 if (e.KeyCode == Keys.D4) { localPlayer.PrimaryWeapon = WeaponType.XM1014; showBuyMenu=false; }
                 if (e.KeyCode == Keys.D5) { localPlayer.PrimaryWeapon = WeaponType.None; showBuyMenu=false; }
                 weaponMenuLabel.Visible = showBuyMenu;
             } else {
                 HandleWeaponSwitch(e.KeyCode);
             }
             
             // Shooting (Simulated)
             if (e.KeyCode == Keys.LButton || e.KeyCode == Keys.ControlKey) // Ctrl or Click if captured
             {
                 recoilPitch -= 2.0f; // Kick up
             }
        }

        private void HandleWeaponSwitch(Keys k)
        {
            if (k == Keys.D1) localPlayer.PrimaryWeapon = WeaponType.AK47;
            if (k == Keys.D2) localPlayer.PrimaryWeapon = WeaponType.Deagle;
            if (k == Keys.D3) localPlayer.PrimaryWeapon = WeaponType.None; // Knife
        }

        private void Update(object sender, EventArgs e)
        {
            float dt = 0.010f;

            // Recoil Recovery
            recoilPitch *= 0.90f;

            // Mouse Look
            if (this.Focused)
            {
                Rectangle bounds = this.Bounds;
                Point center = new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
                Point mousePos = Cursor.Position;
                int dx = mousePos.X - center.X;
                
                if (dx != 0)
                {
                    float sensMultiplier = Game.Settings.MouseSensitivity * 0.022f; 
                    playerAngle += dx * sensMultiplier * (float)(Math.PI / 180.0f) * 5.0f; 
                    Cursor.Position = center;
                }
            }

            // Movement
            Game.Vector2 wishDir = new Game.Vector2(0, 0);
            if (keys[(int)Game.Settings.GetBind("Forward")]) wishDir.Y += 1;
            if (keys[(int)Game.Settings.GetBind("Back")]) wishDir.Y -= 1;
            if (keys[(int)Game.Settings.GetBind("Right")]) wishDir.X += 1;
            if (keys[(int)Game.Settings.GetBind("Left")]) wishDir.X -= 1;

            if (wishDir.Length() > 0) 
            {
                wishDir = wishDir.Normalize();
                walkTime += dt * 1.5f; // Bobbing
            }
            else
            {
                walkTime = 0;
            }
            
            float cos = (float)Math.Cos(playerAngle);
            float sin = (float)Math.Sin(playerAngle);
            Game.Vector2 rotatedWish = new Game.Vector2(
                wishDir.X * cos - wishDir.Y * sin,
                wishDir.X * sin + wishDir.Y * cos
            );
            
            // AutoBhop
            bool jumpHeld = keys[(int)Game.Settings.GetBind("Jump")];
            if (jumpHeld) physics.OnGround = false; else physics.OnGround = true;
            
            physics.ApplyFriction(dt);
            if (physics.OnGround) physics.Accelerate(rotatedWish, Game.Physics.MaxVelocity, Game.Physics.AccelRate, dt);
            else physics.AirAccelerate(rotatedWish, Game.Physics.MaxVelocity, Game.Physics.AirAccelRate, dt);
            physics.Step(dt);

            uiLabel.Text = string.Format("HP: {0} | VEL: {1:000} | {2}", localPlayer.Health, physics.Velocity.Length(), localPlayer.PrimaryWeapon);
            Invalidate(); 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Bot Logic Updates (in Paint for simplicity of loop)
            List<Game.Vector2> botRenderPos = new List<Game.Vector2>();
            double t = DateTime.Now.TimeOfDay.TotalSeconds;

            for(int i=0; i<bots.Count; i++)
            {
                // Bot Pattern: Figure 8
                float scale = 150.0f;
                float bx = (float)Math.Sin(t * 0.5 + i) * scale;
                float by = (float)Math.Cos(t * 0.25 + i) * (scale/2) + 100; // Offset
                botRenderPos.Add(new Game.Vector2(bx, by));
            }

            string weapon = localPlayer.PrimaryWeapon.ToString().ToLower();
            if (localPlayer.PrimaryWeapon == WeaponType.None) weapon = "knife";
            
            // Render Scene with Recoil Offset (Pitch visualized by shifting Y slightly? No, 2.5D doesn't pitch easily, so we shift HUD/Crosshair or Viewport)
            // Ideally we pitch the projection, but here we can just offset the render Y center.
            // Let's pass 0 pitch for now or implement pitch in Renderer later. 
            // We just shake the screen by offseting viewport center?
            // Actually Renderer takes 'height/2' as horizon. We can offset that.
            
            renderer.Render(e.Graphics, physics.Position.X, physics.Position.Y, playerAngle, this.Width, this.Height, botRenderPos, weapon, walkTime);
            
            // Crosshair (Affected by Recoil)
            int cx = Width / 2; 
            int cy = Height / 2 - (int)(recoilPitch * 10); // Recoil moves crosshair up
            
            e.Graphics.DrawLine(Pens.LightGreen, cx - 10, cy, cx + 10, cy);
            e.Graphics.DrawLine(Pens.LightGreen, cx, cy - 10, cx, cy + 10);
            
            e.Graphics.DrawString("AIM DOJO v1.0", new Font("Consolas", 10), Brushes.White, 20, Height - 40);
        }
    }
}
