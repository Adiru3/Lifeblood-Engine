using System;
using System.Collections.Generic;
using System.IO;
using Lifeblood.Rendering;
using Lifeblood.Game;

namespace Lifeblood.Modding
{
    public static class ModLoader
    {
        public static string ModsDirectory = "mods";
        public static Dictionary<string, Mesh> LoadedModels = new Dictionary<string, Mesh>();
        public static Dictionary<string, Texture> LoadedTextures = new Dictionary<string, Texture>();

        public static void Initialize()
        {
            if (!Directory.Exists(ModsDirectory))
            {
                Directory.CreateDirectory(ModsDirectory);
                Console.WriteLine("Created mods directory.");
            }
            
            LoadMods();
        }

        private static void LoadMods()
        {
            string[] modDirs = Directory.GetDirectories(ModsDirectory);
            foreach (var modDir in modDirs)
            {
                Console.WriteLine("Loading Mod: " + Path.GetFileName(modDir));
                
                // Load Models
                string modelsPath = Path.Combine(modDir, "models");
                if (Directory.Exists(modelsPath))
                {
                    foreach (var file in Directory.GetFiles(modelsPath, "*.obj"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file).ToLower();
                        try 
                        {
                            Mesh m = ModelLoader.LoadObj(file);
                            if (LoadedModels.ContainsKey(name)) LoadedModels[name] = m;
                            else LoadedModels.Add(name, m);
                            Console.WriteLine("Loaded Model: " + name);
                        }
                        catch (Exception ex) { Console.WriteLine("Failed to load model: " + file + " Error: " + ex.Message); }
                    }
                }
                
                // Load Textures
                 string texturesPath = Path.Combine(modDir, "textures");
                if (Directory.Exists(texturesPath))
                {
                    foreach (var file in Directory.GetFiles(texturesPath, "*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file).ToLower();
                         try 
                        {
                            Texture t = new Texture(file);
                             if (LoadedTextures.ContainsKey(name)) LoadedTextures[name] = t;
                            else LoadedTextures.Add(name, t);
                            Console.WriteLine("Loaded Texture: " + name);
                        }
                         catch (Exception ex) { Console.WriteLine("Failed to load texture: " + file + " Error: " + ex.Message); }
                    }
                }
            }
        }
        
        public static Mesh GetModel(string name)
        {
            if (LoadedModels.ContainsKey(name)) return LoadedModels[name];
            return null;
        }

        public static Texture GetTexture(string name)
        {
             if (LoadedTextures.ContainsKey(name)) return LoadedTextures[name];
            return null;
        }
    }
}
