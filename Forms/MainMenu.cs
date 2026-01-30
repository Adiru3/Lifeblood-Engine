using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lifeblood.Forms
{
    public class MainMenu : Form
    {
        private Button btnSingleplayer;
        private Button btnMultiplayer;
        private Button btnExit;
        private Label lblTitle;

        public MainMenu()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Lifeblood 3D - Quake Style Shooter";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(20, 20, 25);

            // Заголовок
            lblTitle = new Label();
            lblTitle.Text = "LIFEBLOOD 3D";
            lblTitle.Font = new Font("Arial", 48, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(255, 100, 100);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(200, 100);
            this.Controls.Add(lblTitle);

            // Subtitle
            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Quake World Style Movement • Standalone Version";
            lblSubtitle.Font = new Font("Arial", 12, FontStyle.Italic);
            lblSubtitle.ForeColor = Color.FromArgb(200, 200, 200);
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(220, 180);
            this.Controls.Add(lblSubtitle);

            int buttonY = 280;
            int buttonSpacing = 80;

            // Кнопка одиночной игры
            btnSingleplayer = CreateMenuButton("ОДИНОЧНАЯ ИГРА", buttonY);
            btnSingleplayer.Click += (s, e) => StartGame(false);
            this.Controls.Add(btnSingleplayer);

            // Кнопка мультиплеера
            btnMultiplayer = CreateMenuButton("МУЛЬТИПЛЕЕР", buttonY + buttonSpacing);
            btnMultiplayer.Click += (s, e) => StartGame(true);
            this.Controls.Add(btnMultiplayer);

            // Кнопка Настроек
            Button btnSettings = CreateMenuButton("НАСТРОЙКИ", buttonY + buttonSpacing * 2);
            btnSettings.Click += OpenSettings;
            this.Controls.Add(btnSettings);

            // Кнопка выхода
            btnExit = CreateMenuButton("ВЫХОД", buttonY + buttonSpacing * 3);
            btnExit.Click += (s, e) => Application.Exit();
            this.Controls.Add(btnExit);

            // Информация
            Label lblInfo = new Label();
            lblInfo.Text = "Native OpenGL Engine | Mod Supported";
            lblInfo.Font = new Font("Arial", 10);
            lblInfo.ForeColor = Color.Gray;
            lblInfo.AutoSize = true;
            lblInfo.Location = new Point(260, 550);
            this.Controls.Add(lblInfo);
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            var f = new SettingsMenu();
            f.ShowDialog();
        }

        private Button CreateMenuButton(string text, int y)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Arial", 16, FontStyle.Bold);
            btn.Size = new Size(300, 50);
            btn.Location = new Point(250, y);
            btn.BackColor = Color.FromArgb(50, 50, 60);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 120);
            btn.FlatAppearance.BorderSize = 2;
            btn.Cursor = Cursors.Hand;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(80, 80, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(50, 50, 60);

            return btn;
        }

        private void StartGame(bool isMultiplayer)
        {
            this.Hide();
            var game = new Game3DWindow(isMultiplayer);
            game.FormClosed += (s, e) => this.Show();
            game.ShowDialog();
        }
    }
}
