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

        public delegate void GameTick(float deltaTime);
        public static event GameTick gameTick = new GameTick(_GameTick);

        private static void _GameTick(float deltaTime) { }

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
                gameTick += gameObject.Tick;
            }
        }

        public static GameObject GetGameObjectByID(int id)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.id == id)
                {
                    return gameObject;
                }
            }
            return null;
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
                    switch (parts[2])
                    {
                        case "SL":
                            vertShader = RenderShader.ShadeLighting;
                            break;
                        case "STU":
                            vertShader = RenderShader.ShadeTextureUnit;
                            break;
                    }
                    switch (parts[3])
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
                    go.name = parts[1];
                    go.position = new Vector3(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
                    go.SetRotation(new Vector3(float.Parse(parts[7]), float.Parse(parts[8]), float.Parse(parts[9])));
                    go.scale = new Vector3(float.Parse(parts[10]), float.Parse(parts[11]), float.Parse(parts[12]));
                    go.mesh = OBJImporter.LoadOBJ(parts[13], bool.Parse(parts[14]));
                    if (parts.Length > 15)
                    {
                        go.mesh.SetColour(new Vector3(float.Parse(parts[15]), float.Parse(parts[16]), float.Parse(parts[17])));
                    }
                    if (parts.Length > 18)
                    {
                        string[] imageParts = parts[18].Split(".");
                        go.LoadTexture(imageParts[0], "."+imageParts[1]);
                    }
                    scene.gameObjects.Add(go);
                }
                if (line.StartsWith("/C "))
                {
                    if (line.Replace("/C ", "").StartsWith("S/"))
                    {// base steel engine component
                        string newLine = line.Replace("/C ", "").Replace("S/", "");
                        string[] parts = newLine.Split(":");
                        Type type = Type.GetType("Steel_Engine."+parts[0]);
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
                    else
                    {// custom component
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
                gameTick.Invoke((float)deltaTime);
            }
        }

        public static SteelRay CalculateRay(Vector2 mousePosition)
        {
            float mouseX = mousePosition.X;
            float mouseY = mousePosition.Y;

            int viewportWidth = 1920;
            int viewportHeight = 1040;

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