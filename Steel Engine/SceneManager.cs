using OpenTK.Compute.OpenCL;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class SceneManager
    {
        private static int currentSceneID;
        public static List<Scene> scenes = new List<Scene>();
        public static List<GameObject> gameObjects = new List<GameObject>();
        public static bool gameRunning;

        public static Scene GetActiveScene()
        {
            return scenes[currentSceneID];
        }

        public static void LoadScene(int buildIndex)
        {
            currentSceneID = buildIndex;
            scenes[currentSceneID].Load();
            gameObjects.Clear();
            Scene currentScene = GetActiveScene();
            GameObject[] copy = new GameObject[currentScene.gameObjects.Count];
            currentScene.gameObjects.CopyTo(copy);
            gameObjects.AddRange(copy);

            // load all objects
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Load();
            }
        }

        private static Scene ConstructScene(string[] lines)
        {
            Scene scene = new Scene();
            foreach (string line in lines)
            {
                if (line.StartsWith("/L "))
                {
                    string newLine = line.Replace("/L ", "");
                    string[] parts = newLine.Split(" ");
                    Vector3 pos = new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
                    Vector3 col = LightManager.ColourFromRGB255(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]));
                    scene.AddLight(pos, col, float.Parse(parts[6]));
                }
                if (line.StartsWith("/G "))
                {
                    string newLine = line.Replace("/G ", "");
                    string[] parts = newLine.Split(" ");
                    RenderShader vertShader = RenderShader.ShadeFlat;
                    RenderShader fragShader = RenderShader.ShadeFlat;
                    switch (parts[1])
                    {
                        case "SL":
                            vertShader = RenderShader.ShadeLighting;
                            break;
                        case "STU":
                            vertShader = RenderShader.ShadeTextureUnit;
                            break;
                    }
                    switch (parts[2])
                    {
                        case "SL":
                            fragShader = RenderShader.ShadeLighting;
                            break;
                        case "STU":
                            fragShader = RenderShader.ShadeTextureUnit;
                            break;
                    }
                    GameObject go = new GameObject(vertShader, fragShader);
                    go.id = int.Parse(parts[0]);
                    go.position = new Vector3(float.Parse(parts[3]), float.Parse(parts[4]), float.Parse(parts[5]));
                    go.SetRotation(new Vector3(float.Parse(parts[6]), float.Parse(parts[7]), float.Parse(parts[8])));
                    go.scale = new Vector3(float.Parse(parts[9]), float.Parse(parts[10]), float.Parse(parts[11]));
                    go.mesh = OBJImporter.LoadOBJ(parts[12], bool.Parse(parts[13]));
                    if (parts.Length > 14)
                    {
                        go.mesh.SetColour(new Vector3(float.Parse(parts[14]), float.Parse(parts[15]), float.Parse(parts[16])));
                    }
                    if (parts.Length > 17)
                    {
                        string[] imageParts = parts[17].Split(".");
                        go.LoadTexture(imageParts[0], "."+imageParts[1]);
                    }
                    scene.gameObjects.Add(go);
                }
                if (line.StartsWith("/C "))
                {
                    string newLine = line.Replace("/C ", "");
                    string[] parts = newLine.Split(":");
                    Type type = Type.GetType(parts[0].Replace(".cs",""));
                    string[] newParts = new string[parts.Length];
                    parts.ToList().CopyTo(newParts);
                    List<string> newPartsList = newParts.ToList();
                    newPartsList.RemoveAt(0);
                    foreach (string part in newPartsList)
                    {
                        foreach (GameObject obj in scene.gameObjects)
                        {
                            if (obj.id == int.Parse(part))
                            {
                                Component comp = (Component)Activator.CreateInstance(type);
                                obj.components.Add(comp);
                            }
                        }
                    }
                }
            }
            return scene;
        }

        public static void Init()
        {
            foreach (string file in Directory.GetFiles(InfoManager.dataPath + @"\Scenes\"))
            {
                scenes.Add(ConstructScene(File.ReadAllLines(file)));
            }
        }

        public static void Tick(double deltaTime)
        {
            if (gameRunning)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    gameObject.Tick((float)deltaTime);
                }
            }
        }

        public static SteelRay CalculateRay(MouseState mouseState)
        {
            float mouseX = mouseState.X;
            float mouseY = mouseState.Y;

            int viewportWidth = 1920;
            int viewportHeight = 1080;

            // Calculate the NDC (Normalized Device Coordinates)
            float normalizedX = (2.0f * mouseX) / viewportWidth - 1.0f;
            float normalizedY = 1.0f - (2.0f * mouseY) / viewportHeight;
            float normalizedZ = -1.0f; // Depth in NDC space (typically 0 to 1)

            // Get the view and projection matrices from your camera
            Matrix4 viewMatrix = InfoManager.engineCamera.GetViewMatrix();
            Matrix4 projectionMatrix = InfoManager.engineCamera.GetProjectionMatrix();

            // Calculate the inverse view-projection matrix
            Matrix4 inverseViewProjection = Matrix4.Invert(viewMatrix * projectionMatrix);

            // Create a point in homogeneous clip coordinates (HCC)
            Vector4 nearPointHCC = new Vector4(normalizedX, normalizedY, normalizedZ, 1.0f);
            Vector4 farPointHCC = new Vector4(normalizedX, normalizedY, 1.0f, 1.0f);

            // Transform the HCC points to world space
            Vector4 nearPointWorld = nearPointHCC * inverseViewProjection;
            Vector4 farPointWorld = farPointHCC * inverseViewProjection;

            // Divide by W to get the actual 3D points
            Vector3 rayStartWorld = nearPointWorld.Xyz / nearPointWorld.W;
            Vector3 rayEndWorld = farPointWorld.Xyz / farPointWorld.W;

            // Calculate the ray direction
            Vector3 rayDirection = Vector3.Normalize(rayEndWorld - rayStartWorld);

            SteelRay ray = new SteelRay(rayStartWorld, rayDirection);
            ray.stepSize = 0.5f;
            ray.distance = 10f;
            return ray;
        }
    }
}