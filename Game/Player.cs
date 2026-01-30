using System;

namespace Lifeblood.Game
{
    public class Player
    {
        public int ID { get; private set; }
        public string Name { get; set; }
        
        // Economy
        public int Money { get; set; }
        
        // Health & Armor
        public int Health { get; set; }
        public int Armor { get; set; }
        public bool HasHelmet { get; set; }
        
        // Loadout
        public WeaponType PrimaryWeapon { get; set; }
        public WeaponType SecondaryWeapon { get; set; }
        public GadgetType Gadget { get; set; }
        public int GadgetAmmo { get; set; } // Limit 1 for Flash
        
        public Player(int id, string name)
        {
            ID = id;
            Name = name;
            Reset();
            Money = 800; // Start money
        }

        public void Reset()
        {
            Health = 100;
            Armor = 0;
            HasHelmet = false;
            PrimaryWeapon = WeaponType.None;
            SecondaryWeapon = WeaponType.Pistol;
            Gadget = GadgetType.None;
            GadgetAmmo = 0;
        }

        public bool Buy(string itemName)
        {
            int cost = 0;
            
            // Simple string parser for buy commands
            if (itemName == "ak47")
            {
                cost = WeaponDef.Get(WeaponType.Assault).Cost;
                if (ValidPurchase(cost)) { Money -= cost; PrimaryWeapon = WeaponType.Assault; return true; }
            }
            else if (itemName == "deagle")
            {
                cost = WeaponDef.Get(WeaponType.Deagle).Cost;
                if (ValidPurchase(cost)) { Money -= cost; SecondaryWeapon = WeaponType.Deagle; return true; }
            }
            else if (itemName == "helmet")
            {
                cost = 350; // Helmet cost only? or Vest+Helmet? Assuming Vest=650 + Helmet=350 = 1000 total usually.
                            // User spec: "возможность покупки каски... сохраняет получить два хэдшота"
                if (ValidPurchase(cost)) { Money -= cost; Armor = 100; HasHelmet = true; return true; }
            }
             else if (itemName == "bazooka")
            {
                cost = WeaponDef.Get(WeaponType.RocketLauncher).Cost;
                if (ValidPurchase(cost)) { Money -= cost; PrimaryWeapon = WeaponType.RocketLauncher; return true; }
            }
            
            return false;
        }

        private bool ValidPurchase(int cost)
        {
            if (Money >= cost) return true;
            Console.WriteLine("Not enough money!");
            return false;
        }
    }
}
