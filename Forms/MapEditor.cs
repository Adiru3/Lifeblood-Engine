using System;
using System.Drawing;
using System.Windows.Forms;
using Lifeblood.Rendering;
using System.Collections.Generic;

namespace Lifeblood.Forms
{
    public class MapEditor : Form
    {
        private Panel renderPanel;
        // private Renderer3D renderer; // 3D Preview temporarily disabled
        private List<Wall> walls;
        private bool drawing = false;
        private Point startPoint;
        private Color selectedColor = Color.DarkGray; // Default
        
        // Camera for Preview
        // Camera for Preview (Removed)

        public class Wall
        {
            public float X1, Y1, X2, Y2;
            public Color Color;
        }

        public MapEditor()
        {
            this.Text = "Lifeblood - Map Builder (Community Edition)";
            this.Size = new Size(1400, 800);
            
            walls = new List<Wall>();
            // renderer = new Renderer3D();
            
            var split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            this.Controls.Add(split);

            // 2. 2D Editor (Top Down)
            var editorPanel = new Panel();
            editorPanel.Dock = DockStyle.Fill;
            //editorPanel.BackColor = Color.LightGrid; // Needs custom color or draw grid in Paint
            editorPanel.BackColor = Color.White;

            // 1. Sidebar for Tools
            Panel tools = new Panel();
            tools.Dock = DockStyle.Top;
            tools.Height = 100;
            split.Panel1.Controls.Add(tools);
            
            Button btnSave = new Button() { Text = "SAVE MAP", Location = new Point(10, 10) };
            btnSave.Click += SaveMap;
            tools.Controls.Add(btnSave);

            Button btnLoad = new Button() { Text = "LOAD MAP", Location = new Point(100, 10) };
            btnLoad.Click += LoadMap;
            tools.Controls.Add(btnLoad);
            
            Label lblCol = new Label() { Text = "Wall Color:", Location = new Point(200, 15), AutoSize=true };
            tools.Controls.Add(lblCol);
            
            ComboBox cmbColor = new ComboBox();
            cmbColor.Items.AddRange(new object[] { "Gray", "Red", "Blue", "Green", "White" });
            cmbColor.SelectedIndex = 0;
            cmbColor.Location = new Point(270, 10);
            cmbColor.SelectedIndexChanged += (s, e) => {
                string c = cmbColor.SelectedItem.ToString();
                if(c == "Gray") selectedColor = Color.DarkGray;
                if(c == "Red") selectedColor = Color.Crimson;
                if(c == "Blue") selectedColor = Color.SteelBlue;
                if(c == "Green") selectedColor = Color.ForestGreen;
                if(c == "White") selectedColor = Color.WhiteSmoke;
            };
            tools.Controls.Add(cmbColor);

            Button btnClear = new Button() { Text = "CLEAR ALL", Location = new Point(410, 10) };
            btnClear.Click += (s, e) => { walls.Clear(); renderPanel.Invalidate(); editorPanel.Invalidate(); };
            tools.Controls.Add(btnClear);
            
            // Grid Logic
            editorPanel.Paint += (s, e) => {
                // Grid
                Pen grid = new Pen(Color.LightGray);
                for(int i=0; i<editorPanel.Width; i+=20) e.Graphics.DrawLine(grid, i, 0, i, editorPanel.Height);
                for(int i=0; i<editorPanel.Height; i+=20) e.Graphics.DrawLine(grid, 0, i, editorPanel.Width, i);
                
                // Walls
                foreach(var w in walls) {
                    // World to Screen (Scale 0.1f -> 10x zoom for editor?)
                    // Let's say editor is 1:1 map coordinates + offset center
                    float cx = editorPanel.Width/2; float cy = editorPanel.Height/2;
                    e.Graphics.DrawLine(new Pen(w.Color, 3), cx + w.X1, cy + w.Y1, cx + w.X2, cy + w.Y2);
                }
                
                if(drawing) {
                    Point p = editorPanel.PointToClient(Cursor.Position);
                    float cx = editorPanel.Width/2; float cy = editorPanel.Height/2;
                    e.Graphics.DrawLine(Pens.Red, cx + startPoint.X, cy + startPoint.Y, p.X, p.Y);
                }
                
                e.Graphics.DrawString("Left Click+Drag: Draw Wall | WASD: Move Preview Camera", new Font("Arial", 10), Brushes.Black, 5, editorPanel.Height-20);
            };

            editorPanel.MouseDown += (s, e) => { 
                drawing = true; 
                // Screen to World
                float cx = editorPanel.Width/2; float cy = editorPanel.Height/2;
                startPoint = new Point((int)(e.X - cx), (int)(e.Y - cy));
            };
            
            editorPanel.MouseUp += (s, e) => { 
                if(!drawing) return;
                drawing = false; 
                float cx = editorPanel.Width/2; float cy = editorPanel.Height/2;
                float ex = e.X - cx; float ey = e.Y - cy;
                
                // Snap to Grid (20)
                startPoint = Snap(startPoint);
                Point end = Snap(new Point((int)ex, (int)ey));

                walls.Add(new Wall { 
                    X1 = startPoint.X, Y1 = startPoint.Y, 
                    X2 = end.X, Y2 = end.Y, 
                    Color = selectedColor 
                });
                editorPanel.Invalidate();
                renderPanel.Invalidate();
            };

            split.Panel1.Controls.Add(editorPanel);
            editorPanel.BringToFront(); // Ensure below tools

            // 3. 3D Preview (Placeholder)
            renderPanel = new Panel();
            renderPanel.Dock = DockStyle.Fill;
            renderPanel.BackColor = Color.Black;
            renderPanel.Paint += (s, e) => {
                e.Graphics.DrawString("3D PREVIEW OUTDATED - USE GAME MODE", new Font("Consolas", 12), Brushes.Yellow, 10, 10);
            };
            split.Panel2.Controls.Add(renderPanel);
        }

        private Point Snap(Point p)
        {
            int g = 20; // grid size
            return new Point((int)Math.Round((double)p.X/g)*g, (int)Math.Round((double)p.Y/g)*g);
        }

        private void SaveMap(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Filter = "Lifeblood Map (*.lmap)|*.lmap";
                if (sfd.ShowDialog() == DialogResult.OK) {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(sfd.FileName)) {
                        foreach (var w in walls) {
                            writer.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6}", 
                                w.X1, w.Y1, w.X2, w.Y2, w.Color.R, w.Color.G, w.Color.B));
                        }
                    }
                    MessageBox.Show("Map Saved!");
                }
            }
        }

        private void LoadMap(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Lifeblood Map (*.lmap)|*.lmap";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    walls.Clear();
                    string[] lines = System.IO.File.ReadAllLines(ofd.FileName);
                    foreach(string line in lines) {
                        try {
                            string[] parts = line.Split(',');
                            walls.Add(new Wall {
                                X1 = float.Parse(parts[0]),
                                Y1 = float.Parse(parts[1]),
                                X2 = float.Parse(parts[2]),
                                Y2 = float.Parse(parts[3]),
                                Color = Color.FromArgb(int.Parse(parts[4]), int.Parse(parts[5]), int.Parse(parts[6]))
                            });
                        } catch {}
                    }
                    Invalidate(true); // This will invalidate the form, which will cause both panels to repaint
                }
            }
        }
    }
}
