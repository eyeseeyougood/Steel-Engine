using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class OBJParser
    {
        public static Vector3[] ParseVertexData(List<string> vertexData)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (string line in vertexData)
            {
                string[] parts = line.Split(' ');
                Vector3 vertex = new Vector3();
                vertex.X = float.Parse(parts[0]);
                vertex.Y = float.Parse(parts[1]);
                vertex.Z = float.Parse(parts[2]);
                result.Add(vertex);
            }

            return result.ToArray();
        }

        public static Vector3[] ParseFaceData(List<string> faceData)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (string line in faceData)
            {
                string[] parts = line.Split(' ');
                if (parts.Length == 3) // this is a normal un-triangulated face parse
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    result.Add(face);
                }
                else if (parts.Length == 4) // but if the face is a square, a simple cut down into two triangles is made
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    Vector3 face1 = new Vector3();
                    face1.X = float.Parse(parts[0]);
                    face1.Y = float.Parse(parts[2]);
                    face1.Z = float.Parse(parts[3]);
                    result.Add(face);
                    result.Add(face1);
                }
            }

            return result.ToArray();
        }

        public static Vector2[] ParseTextureData(List<string> textureData)
        {
            List<Vector2> result = new List<Vector2>();

            foreach (string line in textureData)
            {
                string[] parts = line.Split(' ');
                Vector2 face = new Vector2();
                face.X = float.Parse(parts[0]);
                face.Y = float.Parse(parts[1]);
                result.Add(face);
            }

            return result.ToArray();
        }

        public static Vector3[] ParseTextureIndices(List<string> textureIndices)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (string line in textureIndices)
            {
                string[] parts = line.Split(' ');
                if (parts.Length == 3) // this is a normal un-triangulated face parse
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    result.Add(face);
                }
                else if (parts.Length == 4) // but if the face is a square, a simple cut down into two triangles is made
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    Vector3 face1 = new Vector3();
                    face1.X = float.Parse(parts[0]);
                    face1.Y = float.Parse(parts[2]);
                    face1.Z = float.Parse(parts[3]);
                    result.Add(face);
                    result.Add(face1);
                }
            }

            return result.ToArray();
        }
        
        public static Vector3[] ParseVertexNormalData(List<string> normalData)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (string line in normalData)
            {
                string[] parts = line.Split(' ');
                Vector3 normal = new Vector3(0, 0, 0);
                normal.X = float.Parse(parts[0]);
                normal.Y = float.Parse(parts[1]);
                normal.Z = float.Parse(parts[2]);
                result.Add(normal);
            }

            return result.ToArray();
        }

        public static Vector3[] ParseVertexNormalIndices(List<string> normalIndices)
        {
            List<Vector3> result = new List<Vector3>();

            foreach (string line in normalIndices)
            {
                string[] parts = line.Split(' ');
                if (parts.Length == 3) // this is a normal un-triangulated face parse
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    result.Add(face);
                }
                else if (parts.Length == 4) // but if the face is a square, a simple cut down into two triangles is made
                {
                    Vector3 face = new Vector3();
                    face.X = float.Parse(parts[0]);
                    face.Y = float.Parse(parts[1]);
                    face.Z = float.Parse(parts[2]);
                    Vector3 face1 = new Vector3();
                    face1.X = float.Parse(parts[2]);
                    face1.Y = float.Parse(parts[3]);
                    face1.Z = float.Parse(parts[0]);
                    result.Add(face);
                    result.Add(face1);
                }
            }

            return result.ToArray();
        }

        public static float[] ConvertToBufferableVertexData(Vector3[] vertexData)
        {
            List<float> result = new List<float>();

            foreach (Vector3 i in vertexData)
            {
                result.Add(i.X);
                result.Add(i.Y);
                result.Add(i.Z);
            }

            return result.ToArray();
        }

        public static int[] ConvertToIndexData(Vector3[] faceData)
        {
            List<int> result = new List<int>();

            foreach (Vector3 i in faceData)
            {
                result.Add((int)i.X);
                result.Add((int)i.Y);
                result.Add((int)i.Z);
            }

            return result.ToArray();
        }

        public static Mesh GenerateBasicTriangleMesh(Vector3[] vertices, Vector3[] indices, Vector2[] texCoords, Vector3[] textureIndices, Vector3[] normals, Vector3[] normalIndices, bool optimised)
        {
            Mesh mesh = new Mesh();

            for (int i = 0; i < indices.Length;)
            {
                SteelVertex vert1 = new SteelVertex(vertices[(int)indices[i].X - 1]);
                SteelVertex vert2 = new SteelVertex(vertices[(int)indices[i].Y - 1]);
                SteelVertex vert3 = new SteelVertex(vertices[(int)indices[i].Z - 1]);
                vert1.SetNormal(normals[(int)normalIndices[i].X - 1]);
                SteelTriangle triangle = new SteelTriangle(vert1, vert2, vert3, mesh);
                triangle.GetVertex(0).AssignUV(texCoords[(int)textureIndices[i].X-1]);
                triangle.GetVertex(1).AssignUV(texCoords[(int)textureIndices[i].Y-1]);
                triangle.GetVertex(2).AssignUV(texCoords[(int)textureIndices[i].Z-1]);
                mesh.AddTriangleRapid(triangle);
                i++;
            }

            if (optimised)
                mesh.MergeDuplicates();

            return mesh;
        }
    }
}
