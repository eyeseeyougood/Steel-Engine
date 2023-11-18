using OpenTK.Compute.OpenCL;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Steel_Engine
{
    public static class SceneManager
    {
        private static int currentSceneID;
        public static List<Scene> scenes = new List<Scene>();
        public static List<GameObject> gameObjects = new List<GameObject>();
        public static List<Camera> cameras = new List<Camera>();
        public static bool gameRunning;

        public delegate void GameTick(float deltaTime);
        public static event GameTick gameTick = new GameTick(_GameTick);

        public delegate void GameLoad();
        public static event GameLoad gameLoad = new GameLoad(_GameLoad);

        private static void _GameTick(float deltaTime) { }
        private static void _GameLoad() { }

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
                gameLoad += gameObject.Load;
                gameTick += gameObject.Tick;
            }
            gameLoad.Invoke();
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

        private static Scene ConstructScene(string[] lines, string path)
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
                    go.SetRotation(new Vector3(MathHelper.DegreesToRadians(float.Parse(parts[7])), MathHelper.DegreesToRadians(float.Parse(parts[8])), MathHelper.DegreesToRadians(float.Parse(parts[9]))));
                    go.scale = new Vector3(float.Parse(parts[10]), float.Parse(parts[11]), float.Parse(parts[12]));
                    go.mesh = OBJImporter.LoadOBJ(parts[13], bool.Parse(parts[14]));
                    if (parts.Length > 15)
                    {
                        go.mesh.SetColour(new Vector3(float.Parse(parts[15]), float.Parse(parts[16]), float.Parse(parts[17])));
                    }
                    if (parts.Length > 18 && vertShader == RenderShader.ShadeTextureUnit)
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
                if (line.StartsWith("/V "))
                {
                    string newLine = line.Replace("/V ", "");
                    string[] parts = newLine.Split(" ");
                    float camX = float.Parse(parts[2]);
                    float camY = float.Parse(parts[3]);
                    float camZ = float.Parse(parts[4]);
                    Vector3 camPos = new Vector3(camX, camY, camZ);
                    Camera newCam = new Camera(camPos, InfoManager.windowSize.X / InfoManager.windowSize.Y);
                    newCam.id = int.Parse(parts[0]);
                    newCam.name = parts[1];
                    newCam.Pitch = (float.Parse(parts[5]));
                    newCam.Yaw = (float.Parse(parts[6]));
                    newCam.Fov = (float.Parse(parts[7]));
                    if (parts[8] == "Orthographic")
                    {
                        newCam.projectionType = ProjectionType.Orthographic;
                    }
                    scene.cameras.Add(newCam);
                }
                if (line.StartsWith("/S[startingCamera] "))
                {
                    string newLine = line.Replace("/S[startingCamera] ", "");
                    scene.startingcameraID = int.Parse(newLine);
                }
                if (line.StartsWith("/S[windowSize] "))
                {
                    string newLine = line.Replace("/S[windowSize] ", "");
                    string[] parts = newLine.Split(" ");
                    scene.startingcameraID = int.Parse(newLine);
                }
            }
            scene.sceneName = path.Split('\\').Last().Split('.').First();
            return scene;
        }

        public static void Init()
        {
            string path = "";
            if (InfoManager.isBuild)
            {
                path = InfoManager.dataPath + @"\Scenes\";
            }
            else
            {
                path = InfoManager.devDataPath + @"\Scenes\";
            }

            foreach (string file in Directory.GetFiles(path))
            {
                scenes.Add(ConstructScene(File.ReadAllLines(file), file));
            }
        }

        public static void ChangeClearColour(Vector3 colour)
        {
            GL.ClearColor(colour.X, colour.Y, colour.Z, 1.0f);
        }

        public static void Tick(double deltaTime)
        {
            if (gameRunning)
            {
                gameTick.Invoke((float)deltaTime);
            }

            if (InputManager.GetKey(Keys.LeftControl) && InputManager.GetKeyDown(Keys.S) && !InfoManager.isBuild)
            {
                SaveChanges();
            }
        }

        private static void SaveChanges()
        {
            string sceneName = GetActiveScene().sceneName;
            string path = InfoManager.devDataPath + @$"\Scenes\{sceneName}.SES";
            if (File.Exists(path))
                File.Delete(path);

            List<string> lines = new List<string>();
            int id = 0;
            foreach (GameObject gameObject in gameObjects)
            {
                string line = "/G ";
                line += id + " ";
                line += gameObject.name + " ";
                line += ShortenRenderShaderName(gameObject.renderShader) + " ";
                line += ShortenRenderShaderName(gameObject.renderShader) + " ";
                line += gameObject.position.X + " ";
                line += gameObject.position.Y + " ";
                line += gameObject.position.Z + " ";
                line += gameObject.eRotation.X + " ";
                line += gameObject.eRotation.Y + " ";
                line += gameObject.eRotation.Z + " ";
                line += gameObject.scale.X + " ";
                line += gameObject.scale.Y + " ";
                line += gameObject.scale.Z + " ";
                if (gameObject.mesh.loadedModel != "")
                {
                    line += gameObject.mesh.loadedModel + " ";
                    line += gameObject.mesh.optimised + " ";
                    line += gameObject.mesh.vertices[0].colour.X + " ";
                    line += gameObject.mesh.vertices[0].colour.Y + " ";
                    line += gameObject.mesh.vertices[0].colour.Z;
                    if (gameObject.renderShader == RenderShader.ShadeTextureUnit)
                    {
                        Texture texture = gameObject.GetLoadedTexture();
                        line += " " + texture.textureName + texture.textureExtension;
                    }
                }
                lines.Add(line);
                id++;
            }
            id = 0;
            foreach (Camera camera in cameras)
            {
                string line = "/V ";
                line += id + " ";
                line += camera.name + " ";
                line += camera.Position.X + " ";
                line += camera.Position.Y + " ";
                line += camera.Position.Z + " ";
                line += camera.Pitch + " ";
                line += camera.Yaw + " ";
                line += camera.Fov + " ";
                line += camera.projectionType.ToString();
                lines.Add(line);
                id++;
            }
            Dictionary<Type, List<int>> sceneComponents = new Dictionary<Type, List<int>>();
            foreach (GameObject gameObject in gameObjects)
            {
                foreach (Component component in gameObject.components)
                {
                    if (sceneComponents.ContainsKey(component.GetType()))
                    {
                        sceneComponents[component.GetType()].Add(gameObject.id);
                    }
                    else
                    {
                        List<int> list = new List<int>();
                        list.Add(gameObject.id);
                        sceneComponents.Add(component.GetType(), list);
                    }
                }
            }
            foreach (KeyValuePair<Type, List<int>> sceneComponent in sceneComponents)
            {
                string line = "/C ";
                string componentName = sceneComponent.Key.Name;
                if (sceneComponent.Key.IsDefined(typeof(SteelComponentAttribute), true))
                {
                    line += "S/";
                }
                else
                {
                    componentName += ".cs";
                }
                line += componentName;
                foreach (int objRef in sceneComponent.Value)
                {
                    line += ":" + objRef.ToString();
                }
                lines.Add(line);
            }
            File.WriteAllLines(path, lines);
        }

        private static string ShortenRenderShaderName(RenderShader shader)
        {
            string result = "";
            switch (shader)
            {
                case RenderShader.ShadeFlat:
                    result = "SF";
                    break;
                case RenderShader.ShadeTextureUnit:
                    result = "STU";
                    break;
                case RenderShader.ShadeLighting:
                    result = "SL";
                    break;
            }
            return result;
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