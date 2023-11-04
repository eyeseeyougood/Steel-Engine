using OpenTK.Mathematics;
using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class InfoManager
    {
        public static string currentDevPath = @"C:\Users\XUSER\source\repos\Steel Engine\Steel Engine\";
        public static string devDataPath = @"C:\Users\XUSER\source\repos\Steel Engine\Steel Engine\Resources\";
        public static string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\Resources";

        public static float gravityStrength = 1f;

        public static Camera engineCamera;

        public static Vector2 windowSize;

        public static GameObject testSphere;

        public static bool isBuild;
    }
}