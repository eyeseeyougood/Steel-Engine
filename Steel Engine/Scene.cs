using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Steel_Engine.Common;

namespace Steel_Engine
{
    public class Scene
    {
        public string sceneName;
        public List<LightObject> lightObjects = new List<LightObject>();
        public List<GameObject> gameObjects = new List<GameObject>();
        public List<Camera> cameras = new List<Camera>();
        public int startingcameraID = 0;
        public Vector2 windowSize = new Vector2(-1f, -1f);
        public bool lockWindowSize = false;

        public void Load()
        {
            LightManager.lights.Clear();
            LightManager.lights.AddRange(lightObjects);
            SceneManager.cameras.Clear();
            SceneManager.cameras.AddRange(cameras);
            SceneManager.cameras[startingcameraID].SetMain();
            if (windowSize != new Vector2(-1f, -1f))
            {
                InfoManager.SetWindowSize(windowSize);
            }
            InfoManager.lockWindowSize = lockWindowSize;
        }

        public void AddLight(Vector3 pos, Vector3 colour, float intensity)
        {
            LightObject light = LightManager.AddLight(pos, colour, intensity);
            lightObjects.Add(light);
        }
    }
}