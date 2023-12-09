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
        public Vector3 vertexNormal { get; private set; } // -1, -1 for none
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
                return (position == p.position) && (colour == p.colour) && (texCoord == p.texCoord) && (vertexNormal == p.vertexNormal);
            }
        }

        public SteelVertex(Vector3 pos)
        {
            position = pos;
            colour = Vector3.One;
            vertexNormal = Vector3.Zero;
        }

        public SteelVertex(Vector3 pos, Vector3 _colour)
        {
            position = pos;
            colour = _colour;
            vertexNormal = Vector3.Zero;
        }

        public SteelVertex(Vector3 pos, Vector3 _colour, Vector3 _normal)
        {
            position = pos;
            colour = _colour;
            vertexNormal = _normal;
        }

        public void SetNormal(Vector3 normal)
        {
            vertexNormal = normal;
        }

        public void AssignUV(Vector2 refPoint)
        {
            texCoord = refPoint;
        }

        public float[] GetVertexData()
        {
            float[] data = new float[11];

            data[0] = position.X;
            data[1] = position.Y;
            data[2] = position.Z;
            data[3] = colour.X;
            data[4] = colour.Y;
            data[5] = colour.Z;
            data[6] = texCoord.X;
            data[7] = texCoord.Y;
            data[8] = vertexNormal.X;
            data[9] = vertexNormal.Y;
            data[10] = vertexNormal.Z;

            return data;
        }

        public float[] GetVertexData(Quaternion preRotation) // again janky af but only used in GUI so probably ok
        {
            float[] data = new float[11];

            Vector3 newPos = position * Matrix3.CreateFromQuaternion(preRotation); // i couldn't be asked to make it rotate the normals so lighting doesn't work with this

            data[0] = newPos.X;
            data[1] = newPos.Y;
            data[2] = newPos.Z;
            data[3] = colour.X;
            data[4] = colour.Y;
            data[5] = colour.Z;
            data[6] = texCoord.X;
            data[7] = texCoord.Y;
            data[8] = vertexNormal.X;
            data[9] = vertexNormal.Y;
            data[10] = vertexNormal.Z;

            return data;
        }
    }
}
