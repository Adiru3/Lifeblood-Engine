using System;

namespace Lifeblood.Game
{
    public class PlayerStats
    {
        public string Name;
        public int Kills;
        public int Deaths;
        public int Assists;
        public int DamageDealt;
        
        public int ShotsFired;
        public int ShotsHit;
        public int Headshots;
        
        public float KDR 
        { 
            get { return Deaths == 0 ? Kills : (float)Kills / Deaths; } 
        }
        
        public float ADR 
        { 
            get { return 0; } // Needs round tracking for proper ADR, or just divide by 1?
        }
        
        public float Accuracy 
        { 
            get { return ShotsFired == 0 ? 0 : (float)ShotsHit / ShotsFired * 100.0f; } 
        }
        
        public float HSPercent 
        { 
            get { return Kills == 0 ? 0 : (float)Headshots / Kills * 100.0f; } 
        }
        
        public int Ping;
        
        public PlayerStats(string name)
        {
            Name = name;
        }
    }
}
