using System;

namespace Lifeblood.Game
{
    public static class Economy
    {
        public const int StartMoney = 800;
        public const int KillReward = 300;
        public const int WinReward = 3250;
        public const int LossReward = 1400;

        public static void Initialize()
        {
            Console.WriteLine("Economy System Loaded.");
        }

        public static void ProcessKill(Player killer, Player victim)
        {
            if (killer != null && killer != victim)
            {
                killer.Money += KillReward;
                Console.WriteLine(killer.Name + " earned $" + KillReward + " for killing " + victim.Name);
            }
        }
    }
}
