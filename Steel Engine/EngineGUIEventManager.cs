using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Steel_Engine
{
    public class EngineGUIEventManager
    {
        public static void XPButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position += new Vector3(1f, 0, 0) * deltaTime;
        }

        public static void XMButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position -= new Vector3(1f, 0, 0) * deltaTime;
        }

        public static void YPButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position += new Vector3(0, 1f, 0) * deltaTime;
        }

        public static void YMButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position -= new Vector3(0, 1f, 0) * deltaTime;
        }

        public static void ZPButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position -= new Vector3(0, 0, 1f) * deltaTime;
        }

        public static void ZMButtonHold(float deltaTime)
        {
            SceneManager.gameObjects[0].position += new Vector3(0, 0, 1f) * deltaTime;
        }
    }
}