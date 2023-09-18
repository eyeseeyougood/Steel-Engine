using OpenTK.Mathematics;
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

        public static void AddLight(Vector3 pos, Vector3 colour)
        {
            LightObject lo = new LightObject();
            lo.position = pos;
            lo.colour = colour;
            lights.Add(lo);
        }

        public static void AddLight(Vector3 pos, Vector3 colour, float intensity)
        {
            LightObject lo = new LightObject();
            lo.position = pos;
            lo.colour = colour;
            lo.intensity = intensity;
            lights.Add(lo);
        }

        public static Vector3 ColourFromRGB255(Vector3 RGB255)
        {
            return RGB255 / 255;
        }
        
        public static Vector3 ColourFromRGB255(float RGB255X, float RGB255Y, float RGB255Z)
        {
            return new Vector3(RGB255X, RGB255Y, RGB255Z) / 255;
        }
    }
}
