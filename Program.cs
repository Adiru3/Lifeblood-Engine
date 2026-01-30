using System;
using System.Windows.Forms;

namespace Lifeblood
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--server")
            {
                var server = new Network.GameServer();
                server.Start();
                
                Console.WriteLine("Press ENTER to stop server...");
                Console.ReadLine();
                
                server.Stop();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Инициализация игровых данных
            Game.Economy.Initialize();
            
            // Запуск главного меню
            Application.Run(new Forms.MainMenu());
        }
    }
}
