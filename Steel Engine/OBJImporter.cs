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
        public static Mesh LoadOBJ(string name, bool optimised) // obj must be triangulated fully for this to work
        {
            string path = InfoManager.dataPath + $@"\Models\{name}.obj";

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

            File.WriteAllLines(InfoManager.dataPath + @$"\Models\{name}.SEO", newObjData.ToArray());

            return newMesh;
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
                string data = line.AsSpan(2, line.Length - 2).ToString();
                string newData = "";
                int passedSlashes = 0;
                foreach (char i in data)
                {
                    bool keepChar = true;
                    if (i == '/') { keepChar = false; passedSlashes++; }
                    if (i == '/' && passedSlashes == 1) { newData += ' '; }
                    if (i == '/' && passedSlashes == 4) { newData += ' '; }
                    if (passedSlashes == 1) { keepChar = false; }
                    if (passedSlashes == 2) { keepChar = false; }
                    if (passedSlashes == 2 && i == ' ') { keepChar = false; passedSlashes++; }
                    if (passedSlashes == 4) { keepChar = false; }
                    if (passedSlashes == 5) { keepChar = false; }
                    if (passedSlashes == 5 && i == ' ') { keepChar = false; passedSlashes++; }
                    if (passedSlashes == 7) { keepChar = false; }
                    if (passedSlashes == 8) { keepChar = false; }
                    if (keepChar) { newData += i; }                   
                }
                newFaceData.Add(newData);
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

            // we have obtained out original face data
            // now we must take it and convert it to the simpler face data required by SteelObjParser

            List<string> newFaceData = new List<string>();
            foreach (string line in originalFaceData)
            {
                string data = line.AsSpan(2, line.Length - 2).ToString();
                string newData = "";
                int passedSlashes = 0;
                foreach (char i in data)
                {
                    bool keepChar = true;
                    if (i == '/') { keepChar = false; passedSlashes++; }
                    if (i == '/' && passedSlashes == 2) { newData += ' '; }
                    if (i == '/' && passedSlashes == 5) { newData += ' '; }
                    if (passedSlashes == 0) { keepChar = false; }
                    if (passedSlashes == 2) { keepChar = false; }
                    if (passedSlashes == 2 && i == ' ') { keepChar = false; passedSlashes++; }
                    if (passedSlashes == 3) { keepChar = false; }
                    if (passedSlashes == 5) { keepChar = false; }
                    if (passedSlashes == 5 && i == ' ') { keepChar = false; passedSlashes++; }
                    if (passedSlashes == 6) { keepChar = false; }
                    if (passedSlashes == 8) { keepChar = false; }
                    if (keepChar) { newData += i; }                   
                }
                newFaceData.Add(newData);
            }

            return newFaceData;
        }
    }
}
