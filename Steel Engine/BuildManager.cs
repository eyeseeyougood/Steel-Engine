using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class BuildManager
    {
        public static bool buildClientServerFiles;

        public static void Lock()
        {
            File.WriteAllText(InfoManager.currentDir + @"\Runtimes\Xq65.txt", "-11O11So11You11Found11It!!");
        }

        public static void UnlockRebuild()
        {
            string file = InfoManager.currentDir + @"\Runtimes\Xq65.txt";
            if (File.Exists(file))
            {
                File.Delete(InfoManager.currentDir + @"\Runtimes\Xq65.txt");
            }
            Directory.Delete(InfoManager.dataPath + @"\Models\", true);
            Directory.Delete(InfoManager.dataPath + @"\Audio\", true);
            Directory.Delete(InfoManager.dataPath + @"\Scenes\", true);
            Directory.Delete(InfoManager.dataPath + @"\Textures\", true);
            Directory.Delete(InfoManager.currentDir + @"\Shaders\Coordinates\", true);
            Directory.Delete(InfoManager.currentDir + @"\Temp\", true);
        }

        private static void CopyFolder(string src, string dst)
        {
            Directory.CreateDirectory(dst);

            foreach (string file in Directory.GetFiles(src))
            {
                FileInfo fileInfo = new FileInfo(file);
                if (!File.Exists(dst + @$"\{fileInfo.Name}"))
                    File.Copy(file, dst + @$"\{fileInfo.Name}");
                else
                {
                    try
                    {
                        File.WriteAllBytes(dst + @$"\{fileInfo.Name}", File.ReadAllBytes(file));
                    }
                    catch { }
                }
            }
        }

        public static void AssembleFiles()
        {
            string currentPath = InfoManager.currentDir + @"\EngineResources\EngineModels";
            string currentDevPath = InfoManager.currentDevPath + @"\EngineResources\EngineModels";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.currentDir + @"\EngineResources\EngineTextures";
            currentDevPath = InfoManager.currentDevPath + @"\EngineResources\EngineTextures";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.dataPath + @"\Models\";
            currentDevPath = InfoManager.devDataPath + @"\Models\";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.dataPath + @"\Scenes\";
            currentDevPath = InfoManager.devDataPath + @"\Scenes\";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.dataPath + @"\Textures\";
            currentDevPath = InfoManager.devDataPath + @"\Textures\";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.dataPath + @"\Audio\";
            currentDevPath = InfoManager.devDataPath + @"\Audio\";
            CopyFolder(currentDevPath, currentPath);

            currentPath = InfoManager.currentDir + @"\Shaders\Coordinates\";
            currentDevPath = InfoManager.currentDevPath + @"\Shaders\Coordinates\";
            CopyFolder(currentDevPath, currentPath);

            Directory.CreateDirectory(InfoManager.currentDir + @"\Temp\");
        }
    }
}
