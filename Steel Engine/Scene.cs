using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace Steel_Engine
{
    public class Scene
    {
        public string sceneName;
        public List<LightObject> lightObjects = new List<LightObject>();
        public List<GameObject> gameObjects = new List<GameObject>();

        public void Load()
        {
            LightManager.lights.Clear();
            LightManager.lights.AddRange(lightObjects);
        }

        public void AddLight(Vector3 pos, Vector3 colour, float intensity)
        {
            LightObject light = LightManager.AddLight(pos, colour, intensity);
            lightObjects.Add(light);
        }
    }
}
