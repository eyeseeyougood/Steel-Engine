using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class TriangulationManager
    {
        public static List<string> TriangulateFaces(List<string> faceData)
        {
            List<string> faces = new List<string>();
            return faces;
        }

        public static Vector3[] TriangulateQuad(Vector3 v1i, Vector3 v2i, Vector3 v3i, Vector3 v4i)
        {
            Vector3[] indices = new Vector3[6];
            indices[0] = v1i;
            indices[1] = v2i;
            indices[2] = v4i;
            indices[3] = v2i;
            indices[4] = v3i;
            indices[5] = v4i;
            // 1 2 4
            // 2 3 4
            return indices;
        }
    }
}
