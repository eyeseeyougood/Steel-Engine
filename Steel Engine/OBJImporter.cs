using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class OBJImporter
    {
        public static Mesh LoadSEO(string name, bool optimised)
        {
            List<string> newVertexData = new List<string>();
            List<string> newFaceData = new List<string>();
            List<string> newTextureData = new List<string>();
            List<string> newTextureIndices = new List<string>();

            string[] lines = File.ReadAllLines(InfoManager.dataPath + $@"\Models\{name}.SEO");
            foreach (string line in lines)
            {
                if (line.StartsWith("v "))
                {
                    newVertexData.Add(line.Replace("v ", ""));
                }
                if (line.StartsWith("f "))
                {
                    newFaceData.Add(line.Replace("f ", ""));
                }
                if (line.StartsWith("tc "))
                {
                    newTextureData.Add(line.Replace("tc ", ""));
                }
                if (line.StartsWith("ti "))
                {
                    newTextureIndices.Add(line.Replace("ti ", ""));
                }
            }

            Vector3[] parsedVertexData = OBJParser.ParseVertexData(newVertexData);
            Vector3[] parsedFaceData = OBJParser.ParseFaceData(newFaceData);
            Vector2[] parsedTextureData = OBJParser.ParseTextureData(newTextureData);
            Vector3[] parsedTextureIndices = OBJParser.ParseTextureIndices(newTextureIndices);

            Mesh newMesh = OBJParser.GenerateBasicTriangleMesh(parsedVertexData, parsedFaceData, parsedTextureData, parsedTextureIndices, optimised);

            return newMesh;
        }

        public static Mesh LoadOBJ(string name, bool optimised) // obj must be triangulated fully for this to work
        {
            string path = InfoManager.dataPath + $@"\Models\{name}.obj";

            if (File.Exists(InfoManager.dataPath + $@"\Models\{name}.SEO"))
            {
                return LoadSEO(name, optimised);                
            }

            // Data sanitisation
            List<string> newObjData = new List<string>();
            foreach (string line in File.ReadLines(path))
            {
                bool keepLine = true;
                if (line.StartsWith("#")) { keepLine = false; }
                if (line.StartsWith("vn")) { keepLine = false; }
                if (line.StartsWith("mtllib")) { keepLine = false; }
                if (line.StartsWith("usemtl")) { keepLine = false; }
                if (line.StartsWith("o")) { keepLine = false; }
                if (int.TryParse(line.ElementAt(0).ToString(), out int result)) { keepLine = false; }
                if (keepLine) { newObjData.Add(line); }
            }

            List<string> newVertexData = ExtractVertexData(newObjData);
            List<string> newFaceData = ExtractFaceData(newObjData);
            List<string> newTextureData = ExtractTextureData(newObjData);
            List<string> newTextureIndices = ExtractTextureIndices(newObjData);

            List<string> finalText = new List<string>();
            foreach (string line in newVertexData)
            {
                finalText.Add("v " + line);
            }
            foreach (string line in newFaceData)
            {
                finalText.Add("f " + line);
            }
            foreach (string line in newTextureData)
            {
                finalText.Add("tc " + line);
            }
            foreach (string line in newTextureIndices)
            {
                finalText.Add("ti " + line);
            }
            File.WriteAllLines(InfoManager.dataPath + @$"\Models\{name}.SEO", finalText.ToArray());

            return LoadSEO(name, optimised);
        }

        public static Mesh LoadOBJFromPath(string path, bool optimised) // obj must be triangulated fully for this to work
        {
            // Data sanitisation
            List<string> newObjData = new List<string>();
            foreach (string line in File.ReadLines(path))
            {
                bool keepLine = true;
                if (line.StartsWith("#")) { keepLine = false; }
                if (line.StartsWith("vn")) { keepLine = false; }
                if (line.StartsWith("mtllib")) { keepLine = false; }
                if (line.StartsWith("usemtl")) { keepLine = false; }
                if (line.StartsWith("o")) { keepLine = false; }
                if (int.TryParse(line.ElementAt(0).ToString(), out int result)) { keepLine = false; }
                if (keepLine) { newObjData.Add(line); }
            }

            List<string> newVertexData = ExtractVertexData(newObjData);
            List<string> newFaceData = ExtractFaceData(newObjData);
            List<string> newTextureData = ExtractTextureData(newObjData);
            List<string> newTextureIndices = ExtractTextureIndices(newObjData);

            Vector3[] parsedVertexData = OBJParser.ParseVertexData(newVertexData);
            Vector3[] parsedFaceData = OBJParser.ParseFaceData(newFaceData);
            Vector2[] parsedTextureData = OBJParser.ParseTextureData(newTextureData);
            Vector3[] parsedTextureIndices = OBJParser.ParseTextureIndices(newTextureIndices);

            Mesh newMesh = OBJParser.GenerateBasicTriangleMesh(parsedVertexData, parsedFaceData, parsedTextureData, parsedTextureIndices, optimised);

            File.WriteAllLines(path.Split('.')[0] + ".SEO", newObjData.ToArray());

            return newMesh;
        }

        private static List<string> ExtractFaceData(List<string> objData)
        {
            List<string> originalFaceData = new List<string>();
            foreach (string line in objData)
            {
                if (line.StartsWith("f")) { originalFaceData.Add(line); }
            }

            // we have obtained out original face data
            // now we must take it and convert it to the simpler face data required by SteelObjParser

            List<string> newFaceData = new List<string>();
            foreach (string line in originalFaceData)
            {
                string data = line.Replace("f ", "");
                string newFace = "";
                string[] faces = data.Split(' ');
                foreach (string face in faces)
                {
                    newFace += face.Split('/')[0] + " ";
                }
                string reformatted = "";
                int index = 0;
                foreach (char c in newFace)
                {
                    if (index == newFace.Length - 1)
                    {
                        break;
                    }
                    reformatted += c;
                    index++;
                }

                newFaceData.Add(reformatted);
            }

            return newFaceData;
        }
        
        private static List<string> ExtractVertexData(List<string> objData)
        {
            List<string> newVertexData = new List<string>();
            foreach (string line in objData)
            {
                if (line.StartsWith("v ")) { newVertexData.Add(line.Replace("v ", "")); }
            }

            return newVertexData;
        }

        public static List<string> ExtractTextureData(List<string> objData)
        {
            List<string> textureCoords = new List<string>();
            foreach (string line in objData)
            {
                if (line.StartsWith("vt "))
                {
                    textureCoords.Add(line.Replace("vt ", ""));
                }
            }
            return textureCoords;
        }

        private static List<string> ExtractTextureIndices(List<string> objData)
        {
            List<string> originalFaceData = new List<string>();
            foreach (string line in objData)
            {
                if (line.StartsWith("f")) { originalFaceData.Add(line); }
            }

            List<string> newFaceData = new List<string>();
            foreach (string line in originalFaceData)
            {
                string data = line.Replace("f ", "");
                string newFace = "";
                string[] faces = data.Split(' ');
                foreach (string face in faces)
                {
                    newFace += face.Split('/')[1] + " ";
                }
                string reformatted = "";
                int index = 0;
                foreach (char c in newFace)
                {
                    if (index == newFace.Length - 1)
                    {
                        break;
                    }
                    reformatted += c;
                    index++;
                }

                newFaceData.Add(reformatted);
            }

            return newFaceData;
        }
    }
}
