using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class SteelVertex
    {
        public Vector3 position;
        public Vector3 colour;
        public Vector2 texCoord { get; private set; } // -1, -1 for none
        public override bool Equals(object? obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                SteelVertex p = (SteelVertex)obj;
                return (position == p.position) && (colour == p.colour) && (texCoord == p.texCoord);
            }
        }

        public SteelVertex(Vector3 pos)
        {
            position = pos;
            colour = Vector3.One;
        }

        public SteelVertex(Vector3 pos, Vector3 _colour)
        {
            position = pos;
            colour = _colour;
        }

        public void AssignUV(Vector2 refPoint)
        {
            texCoord = refPoint;
        }

        public float[] GetVertexData()
        {
            float[] data = new float[8];

            data[0] = position.X;
            data[1] = position.Y;
            data[2] = position.Z;
            data[3] = colour.X;
            data[4] = colour.Y;
            data[5] = colour.Z;
            data[6] = texCoord.X;
            data[7] = texCoord.Y;

            return data;
        }
    }
}
