﻿using OpenTK.Mathematics;
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
            List<string> newVertexNormalData = new List<string>();
            List<string> newVertexNormalIndices = new List<string>();

            string path = "";
            if (InfoManager.isBuild)
            {
                path = InfoManager.dataPath + $@"\Models\{name}.SEO";
            }
            else
            {
                path = InfoManager.devDataPath + $@"\Models\{name}.SEO";
            }

            string[] lines = File.ReadAllLines(path);
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
                if (line.StartsWith("nd "))
                {
                    newVertexNormalData.Add(line.Replace("nd ", ""));
                }
                if (line.StartsWith("ni "))
                {
                    newVertexNormalIndices.Add(line.Replace("ni ", ""));
                }
            }

            if (name == "Cube")
            {
            }

            Vector3[] parsedVertexData = OBJParser.ParseVertexData(newVertexData);
            Vector3[] parsedFaceData = OBJParser.ParseFaceData(newFaceData);
            Vector2[] parsedTextureData = OBJParser.ParseTextureData(newTextureData);
            Vector3[] parsedTextureIndices = OBJParser.ParseTextureIndices(newTextureIndices);
            Vector3[] parsedVertexNormalData = OBJParser.ParseVertexNormalData(newVertexNormalData);
            Vector3[] parsedVertexNormalIndices = OBJParser.ParseVertexNormalIndices(newVertexNormalIndices);

            Mesh newMesh = OBJParser.GenerateBasicTriangleMesh(parsedVertexData, parsedFaceData, parsedTextureData, parsedTextureIndices, parsedVertexNormalData, parsedVertexNormalIndices, optimised);

            newMesh.loadedModel = name;
            newMesh.optimised = optimised;

            return newMesh;
        }

        public static Mesh LoadOBJ(string name, bool optimised)
        {
            string path = InfoManager.usingDataPath + $@"\Models\{name}.obj";
            string pathSEO = InfoManager.usingDataPath + $@"\Models\{name}.SEO";

            if (File.Exists(pathSEO))
            {
                return LoadSEO(name, optimised);                
            }

            // Data sanitisation
            List<string> newObjData = new List<string>();
            foreach (string line in File.ReadLines(path))
            {
                bool keepLine = true;
                if (line.StartsWith("#")) { keepLine = false; }
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
            List<string> newVertexNormalData = ExtractVertexNormalData(newObjData);
            List<string> newVertexNormalIndices = ExtractVertexNormalIndices(newObjData);

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
            foreach (string line in newVertexNormalData)
            {
                finalText.Add("nd " + line);
            }
            foreach (string line in newVertexNormalIndices)
            {
                finalText.Add("ni " + line);
            }

            File.WriteAllLines(pathSEO, finalText.ToArray());

            return LoadSEO(name, optimised);
        }

        public static Mesh LoadOBJFromPath(string path, bool optimised)
        {
            // Data sanitisation
            List<string> newObjData = new List<string>();
            foreach (string line in File.ReadLines(path))
            {
                bool keepLine = true;
                if (line.StartsWith("#")) { keepLine = false; }
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
            List<string> newVertexNormalData = ExtractVertexNormalData(newObjData);
            List<string> newVertexNormalIndices = ExtractVertexNormalIndices(newObjData);

            Vector3[] parsedVertexData = OBJParser.ParseVertexData(newVertexData);
            Vector3[] parsedFaceData = OBJParser.ParseFaceData(newFaceData);
            Vector2[] parsedTextureData = OBJParser.ParseTextureData(newTextureData);
            Vector3[] parsedTextureIndices = OBJParser.ParseTextureIndices(newTextureIndices);
            Vector3[] parsedVertexNormalData = OBJParser.ParseVertexNormalData(newVertexNormalData);
            Vector3[] parsedVertexNormalIndices = OBJParser.ParseVertexNormalIndices(newVertexNormalIndices);

            Mesh newMesh = OBJParser.GenerateBasicTriangleMesh(parsedVertexData, parsedFaceData, parsedTextureData, parsedTextureIndices, parsedVertexNormalData, parsedVertexNormalIndices, optimised);

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
        
        private static List<string> ExtractVertexNormalData(List<string> objData)
        {
            List<string> originalNormalData = new List<string>();
            foreach (string line in objData)
            {
                if (line.StartsWith("vn ")) { originalNormalData.Add(line); }
            }

            List<string> newNormalData = new List<string>();
            foreach (string line in originalNormalData)
            {
                string data = line.Replace("vn ", "");
                newNormalData.Add(data);
            }

            return newNormalData;
        }
        
        private static List<string> ExtractVertexNormalIndices(List<string> objData)
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
                    newFace += face.Split('/')[2] + " ";
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
