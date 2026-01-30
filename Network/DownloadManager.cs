using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Lifeblood.Network
{
    public class DownloadManager
    {
        public static string DownloadsPath = "bin/downloads/";
        
        public static void Initialize()
        {
            if (!Directory.Exists(DownloadsPath))
                Directory.CreateDirectory(DownloadsPath);
        }

        public static bool CheckAndDownload(string resourceName, string serverUrl)
        {
            string localPath = Path.Combine(DownloadsPath, resourceName);
            
            if (File.Exists(localPath)) return true;

            // Missing file! Simulation of download
            Console.WriteLine("Missing resource: " + resourceName);
            Console.WriteLine("Downloading from " + serverUrl + "...");
            
            try
            {
                // In a real implementation: WebClient / HTTP GET
                // new WebClient().DownloadFile(serverUrl + "/files/" + resourceName, localPath);
                
                // For MVP: Create a dummy file or try to copy from content
                if (resourceName.EndsWith(".obj"))
                {
                    File.Copy("bin/content/models/enemy.obj", localPath); // Fallback
                }
                else if (resourceName.EndsWith(".png"))
                {
                     File.Copy("bin/content/textures/wall.png", localPath); // Fallback
                }
                else
                {
                    File.WriteAllText(localPath, "Downloaded Content");
                }
                
                Console.WriteLine("Download Complete: " + localPath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Download Failed: " + ex.Message);
                return false;
            }
        }
    }
}
