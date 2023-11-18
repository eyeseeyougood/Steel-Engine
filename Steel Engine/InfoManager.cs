using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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
        public static string currentDevPath = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\";
        public static string devDataPath = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\Resources\";
        public static string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\Resources";

        public static float gravityStrength = 1f;

        public static Camera engineCamera;

        public static Vector2 windowSize;

        public static GameObject testObject;

        public static bool isBuild;

        public static bool lockWindowSize;

        private static Vector2 windowLock;
        private static WindowState windowStateLock;

        public delegate void OnSetWindowSize(int width, int height);
        public static event OnSetWindowSize setWindowSize;

        public delegate void OnSetWindowState(WindowState state);
        public static event OnSetWindowState setWindowState;

        public static void Tick(float deltaTime)
        {
            if (lockWindowSize)
            {
                SetWindowState(windowStateLock);
                SetWindowSize(windowLock);
            }
        }

        public static void SetWindowSize(Vector2 windowSize)
        {
            windowLock = windowSize;
            setWindowSize.Invoke((int)windowSize.X, (int)windowSize.Y);
        }

        public static void SetWindowState(WindowState state)
        {
            windowStateLock = state;
            setWindowState.Invoke(state);
        }
    }
}