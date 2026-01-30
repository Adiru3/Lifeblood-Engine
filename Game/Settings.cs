using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Lifeblood.Game
{
    public struct Vector2 
    {
        public float X, Y;
        public Vector2(float x, float y) { X = x; Y = y; }
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.X + b.X, a.Y + b.Y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.X - b.X, a.Y - b.Y); }
        public static Vector2 operator *(Vector2 a, float d) { return new Vector2(a.X * d, a.Y * d); }
        public float Length() { return (float)Math.Sqrt(X*X + Y*Y); }
        public Vector2 Normalize() {
            float len = Length();
            return len > 0 ? new Vector2(X / len, Y / len) : new Vector2(0,0);
        }
        public static float Dot(Vector2 a, Vector2 b) { return a.X * b.X + a.Y * b.Y; }
    }

    public static class Settings
    {
        // Identity
        public static string Nickname = "Player";
        
        // Video
        public static int ResolutionWidth = 1280;
        public static int ResolutionHeight = 720;
        public static bool Fullscreen = false;

        // Mouse
        public static float MouseSensitivity = 2.5f; 
        public static float ZoomSensitivityRatio = 1.0f;
        public static float FOV = 110.0f;
        
        // Crosshair (CS Style)
        public static int CrosshairStyle = 4; // 4 = Classic Static
        public static float CrosshairSize = 5.0f;
        public static float CrosshairGap = 1.5f;
        public static float CrosshairThickness = 1.0f;
        public static bool CrosshairDot = false;
        public static float CrosshairAlpha = 255.0f;
        public static System.Drawing.Color CrosshairColor = System.Drawing.Color.Green;
        
        // Keys
        public static Dictionary<string, Keys> KeyBinds = new Dictionary<string, Keys>() {
            { "Forward", Keys.W },
            { "Back", Keys.S },
            { "Left", Keys.A },
            { "Right", Keys.D },
            { "Jump", Keys.Space },
            { "Crouch", Keys.ControlKey },
            { "Walk", Keys.ShiftKey },
            { "Duck", Keys.ControlKey },
            { "Reload", Keys.R },
            { "Scoreboard", Keys.Tab }, // Added Tab
            { "Slot1", Keys.D1 },
            { "Slot2", Keys.D2 },
            { "Slot3", Keys.D3 },
            { "Slot4", Keys.D4 },
            { "Slot5", Keys.D5 },
            { "Slot6", Keys.D6 },
            { "Slot7", Keys.D7 }
        };

        public static void SetBind(string action, Keys key)
        {
            if (KeyBinds.ContainsKey(action)) KeyBinds[action] = key;
        }

        public static Keys GetBind(string action)
        {
            return KeyBinds.ContainsKey(action) ? KeyBinds[action] : Keys.None;
        }
    }
}
