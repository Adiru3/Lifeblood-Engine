namespace Lifeblood.Game
{
    public enum WeaponType
    {
        None,
        Knife,
        Pistol,    // Glock/USP
        Deagle,    // High damage pistol
        Shotgun,
        Assault,   // AK/M4
        Scout,     // Light Sniper (High Mobility)
        RocketLauncher, // Quake classic
        Grenade    // HE
    }

    public enum GadgetType
    {
        None,
        Flashbang,
        Smoke
    }

    public class WeaponDef
    {
        public string Name;
        public int Damage;
        public float FireRate; // Seconds between shots
        public bool IsHitscan;
        public int ProjectileSpeed; // 0 if hitscan
        public float Range;
        public int Cost; // Added for Economy

        public static WeaponDef Get(WeaponType type)
        {
            switch (type)
            {
                case WeaponType.Knife: 
                    return new WeaponDef { Name = "Knife", Damage = 50, FireRate = 0.5f, IsHitscan = true, Range = 100, Cost = 0 };
                case WeaponType.Pistol: 
                    return new WeaponDef { Name = "USP-S", Damage = 25, FireRate = 0.15f, IsHitscan = true, Range = 2000, Cost = 500 };
                case WeaponType.Deagle: 
                    return new WeaponDef { Name = "Desert Eagle", Damage = 60, FireRate = 0.3f, IsHitscan = true, Range = 4000, Cost = 700 };
                case WeaponType.Shotgun: 
                    return new WeaponDef { Name = "M3 Super 90", Damage = 15, FireRate = 0.9f, IsHitscan = true, Range = 1500, Cost = 1200 }; // Per pellet logic needed later
                case WeaponType.Assault: 
                    return new WeaponDef { Name = "AK-47", Damage = 35, FireRate = 0.1f, IsHitscan = true, Range = 5000, Cost = 2700 };
                case WeaponType.Scout: 
                    return new WeaponDef { Name = "SSG 08", Damage = 75, FireRate = 1.2f, IsHitscan = true, Range = 10000, Cost = 1700 };
                case WeaponType.RocketLauncher: 
                    return new WeaponDef { Name = "Rocket Launcher", Damage = 100, FireRate = 0.8f, IsHitscan = false, ProjectileSpeed = 1000, Cost = 4000 };
                case WeaponType.Grenade:
                    return new WeaponDef { Name = "HE Grenade", Damage = 90, FireRate = 1.0f, IsHitscan = false, ProjectileSpeed = 700, Cost = 300 };
                default: 
                    return new WeaponDef { Name = "Hands", Damage = 0, Cost = 0 };
            }
        }
    }
}
