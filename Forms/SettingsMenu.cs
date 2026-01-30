using System;
using System.Drawing;
using System.Windows.Forms;
using Lifeblood.Game;

namespace Lifeblood.Forms
{
    public class SettingsMenu : Form
    {
        private TabControl tabControl;
        
        // General Controls
        private TextBox txtNickname;
        
        // Video Controls
        private TextBox txtResW, txtResH;
        private CheckBox chkFullscreen;
        private TextBox txtFov;
        
        // Crosshair Controls
        private TextBox txtCrossSize, txtCrossGap, txtCrossAlpha;
        private CheckBox chkCrossDot;
        private Button btnCrossColor;
        
        // Input Controls
        private TextBox txtSens;

        public SettingsMenu()
        {
            this.Text = "Lifeblood - Configuration";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Top;
            tabControl.Height = 480;
            
            TabPage tabGen = new TabPage("General");
            TabPage tabVideo = new TabPage("Video / Crosshair");
            TabPage tabInput = new TabPage("Controls");
            
            BuildGeneralTab(tabGen);
            BuildVideoTab(tabVideo);
            BuildInputTab(tabInput);
            
            tabControl.TabPages.Add(tabGen);
            tabControl.TabPages.Add(tabVideo);
            tabControl.TabPages.Add(tabInput);
            this.Controls.Add(tabControl);
            
            Button btnSave = new Button();
            btnSave.Text = "SAVE & CLOSE";
            btnSave.Location = new Point(150, 500);
            btnSave.Size = new Size(200, 40);
            btnSave.BackColor = Color.Crimson;
            btnSave.Flatten();
            btnSave.Click += SaveSettings;
            this.Controls.Add(btnSave);
        }

        private void BuildGeneralTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(40, 40, 40);
            int y = 20;
            AddLabel(tab, "Nickname:", 20, y);
            txtNickname = AddTextBox(tab, Settings.Nickname, 150, y);
        }

        private void BuildVideoTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(40, 40, 40);
            int y = 20;
            
            AddLabel(tab, "Resolution (WxH):", 20, y);
            txtResW = AddTextBox(tab, Settings.ResolutionWidth.ToString(), 150, y);
            txtResH = AddTextBox(tab, Settings.ResolutionHeight.ToString(), 260, y);
            y += 40;
            
            AddLabel(tab, "FOV:", 20, y);
            txtFov = AddTextBox(tab, Settings.FOV.ToString(), 150, y);
            y += 40;
            
            // Crosshair
            AddLabel(tab, "--- CROSSHAIR ---", 20, y); y += 30;
            
            AddLabel(tab, "Size:", 20, y);
            txtCrossSize = AddTextBox(tab, Settings.CrosshairSize.ToString(), 150, y);
            y += 30;
            
            AddLabel(tab, "Gap:", 20, y);
            txtCrossGap = AddTextBox(tab, Settings.CrosshairGap.ToString(), 150, y);
            y += 30;
            
            AddLabel(tab, "Dot:", 20, y);
            chkCrossDot = new CheckBox();
            chkCrossDot.Checked = Settings.CrosshairDot;
            chkCrossDot.Location = new Point(150, y);
            tab.Controls.Add(chkCrossDot);
            y += 30;
            
            AddLabel(tab, "Color:", 20, y);
            btnCrossColor = new Button();
            btnCrossColor.BackColor = Settings.CrosshairColor;
            btnCrossColor.Location = new Point(150, y);
            btnCrossColor.Size = new Size(50, 23);
            btnCrossColor.Click += (s, e) => {
                ColorDialog cd = new ColorDialog();
                if (cd.ShowDialog() == DialogResult.OK) btnCrossColor.BackColor = cd.Color;
            };
            tab.Controls.Add(btnCrossColor);
        }

        private void BuildInputTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(40, 40, 40);
            int y = 20;
            
            AddLabel(tab, "Sensitivity:", 20, y);
            txtSens = AddTextBox(tab, Settings.MouseSensitivity.ToString(), 150, y);
            y += 40;
            
            AddLabel(tab, "KEY BINDINGS (Edit Config File for now)", 20, y);
            y += 30;
            
            foreach(var kvp in Settings.KeyBinds)
            {
                AddLabel(tab, kvp.Key + ": " + kvp.Value.ToString(), 40, y);
                y += 20;
            }
        }

        private void SaveSettings(object sender, EventArgs e)
        {
            Settings.Nickname = txtNickname.Text;
            
            int w, h;
            if (int.TryParse(txtResW.Text, out w)) Settings.ResolutionWidth = w;
            if (int.TryParse(txtResH.Text, out h)) Settings.ResolutionHeight = h;
            
            float f;
            if (float.TryParse(txtFov.Text, out f)) Settings.FOV = f;
            if (float.TryParse(txtSens.Text, out f)) Settings.MouseSensitivity = f;
            
            if (float.TryParse(txtCrossSize.Text, out f)) Settings.CrosshairSize = f;
            if (float.TryParse(txtCrossGap.Text, out f)) Settings.CrosshairGap = f;
            Settings.CrosshairDot = chkCrossDot.Checked;
            Settings.CrosshairColor = btnCrossColor.BackColor;
            
            this.Close();
        }

        private void AddLabel(TabPage tab, string text, int x, int y)
        {
            Label l = new Label();
            l.Text = text;
            l.Location = new Point(x, y);
            l.AutoSize = true;
            l.ForeColor = Color.White;
            tab.Controls.Add(l);
        }
        
        private TextBox AddTextBox(TabPage tab, string val, int x, int y)
        {
            TextBox t = new TextBox();
            t.Text = val;
            t.Location = new Point(x, y);
            t.Width = 100;
            tab.Controls.Add(t);
            return t;
        }
    }
    
    static class Ext { public static void Flatten(this Button b) { b.FlatStyle = FlatStyle.Flat; b.FlatAppearance.BorderSize = 0; } }
}
