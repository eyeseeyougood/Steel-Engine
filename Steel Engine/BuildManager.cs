using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class BuildManager
    {
        public static void Lock()
        {
            File.WriteAllText(InfoManager.currentDir + @"\Runtimes\Xq65.txt", "-11O11So11You11Found11It!!");
        }

        public static void AssembleFiles()
        {
            string currentPath = InfoManager.currentDir + @"\EngineResources\EngineModels";
            string currentDevPath = InfoManager.currentDevPath + @"\EngineResources\EngineModels";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            currentPath = InfoManager.currentDir + @"\EngineResources\EngineTextures";
            currentDevPath = InfoManager.currentDevPath + @"\EngineResources\EngineTextures";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            currentPath = InfoManager.dataPath + @"\Models\";
            currentDevPath = InfoManager.devDataPath + @"\Models\";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            currentPath = InfoManager.dataPath + @"\Scenes\";
            currentDevPath = InfoManager.devDataPath + @"\Scenes\";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            currentPath = InfoManager.dataPath + @"\Textures\";
            currentDevPath = InfoManager.devDataPath + @"\Textures\";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            currentPath = InfoManager.currentDir + @"\Shaders\Coordinates\";
            currentDevPath = InfoManager.currentDevPath + @"\Shaders\Coordinates\";
            Directory.CreateDirectory(currentPath);

            foreach (string file in Directory.GetFiles(currentDevPath))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(currentPath + @$"\{fileInfo.Name}"))
                    File.Copy(file, currentPath + @$"\{fileInfo.Name}");
            }

            Directory.CreateDirectory(InfoManager.currentDir + @"\Temp\");
        }
    }
}
