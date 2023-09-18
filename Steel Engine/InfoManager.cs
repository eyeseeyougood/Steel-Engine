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
        public static string currentDir = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\";
        public static string dataPath = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\Resources\";

        public static float gravityStrength = 9.81f;

        public static Camera engineCamera;

        public static Vector2 windowSize;
    }
}