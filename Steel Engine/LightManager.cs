using OpenTK.Mathematics;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class LightObject
    {
        public Vector3 position;
        public Vector3 colour;
        public float intensity = 1;
    }

    public static class LightManager
    {
        public static List<LightObject> lights = new List<LightObject>();
        public static Dictionary<GUIWorldImage, int> lightGizmos = new Dictionary<GUIWorldImage, int>();

        public static void Tick()
        {
            // update gizmos
            foreach (KeyValuePair<GUIWorldImage, int> gizmo in lightGizmos)
            {
                if (lights.Count > gizmo.Value)
                {
                    gizmo.Key.addedPosition = lights[gizmo.Value].position;
                }
            }
        }

        public static void RenderGizmos()
        {
            foreach (KeyValuePair<GUIWorldImage, int> gizmo in lightGizmos)
            {
                gizmo.Key.Render();
            }
        }

        private static void AddLightGizmo(LightObject lo)
        {
            GUIWorldImage go = new GUIWorldImage(lo.position, Vector2.Zero, new Vector2(0.2f, 0.2f), InfoManager.usingDirectory + "/EngineResources/EngineTextures/Gizmo_Light.png");
            lightGizmos.Add(go, lights.IndexOf(lo));
        }

        public static LightObject AddLight(Vector3 pos, Vector3 colour)
        {
            LightObject lo = new LightObject();
            lo.position = pos;
            lo.colour = colour;
            lights.Add(lo);
            AddLightGizmo(lo);
            return lo;
        }

        public static LightObject AddLight(Vector3 pos, Vector3 colour, float intensity)
        {
            LightObject lo = new LightObject();
            lo.position = pos;
            lo.colour = colour;
            lo.intensity = intensity;
            lights.Add(lo);
            AddLightGizmo(lo);
            return lo;
        }
    }
}
