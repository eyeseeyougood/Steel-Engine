using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;

namespace Steel_Engine
{
    public class EngineGUIEventManager
    {
        public static void ToggleComponentViewEvent(string buttonName)
        {
            if (GUIManager.selectedhierarchyObject != null)
            {
                // make pop-up
                GUIManager.GetElementByName("componentsView").visible = !GUIManager.GetElementByName("componentsView").visible;
                GUIManager.GetElementByName("componentsView").active = GUIManager.GetElementByName("componentsView").visible;
            }
        }

        public static void AddComponentEvent(string buttonName)
        {
            // find type from button name
            Type component = Type.GetType("Steel_Engine."+buttonName.Split(' ')[0]);
            if (component == null)
            {
                component = Type.GetType(buttonName.Split(' ')[0]);
            }
            // instantiate the component
            Component comp = (Component)Activator.CreateInstance(component);
            // add it to the object and initiate it
            GameObject go = SceneManager.gameObjects[int.Parse(GUIManager.selectedhierarchyObject.name.Split(' ')[0])];
            go.AddComponent(comp); // AddComponent initialises the component
            // refresh inspector
            GUIManager.RefreshInspectorMenu();
        }

        public static void UnlockRebuild(float deltaTime, string buttonName)
        {
            BuildManager.UnlockRebuild();
        }

        public static void CreateEmpty(string buttonName)
        {
            GUIScrollView hierarchy = (GUIScrollView)GUIManager.GetElementByName("hierarchyBG");
            GameObject go = new GameObject(RenderShader.ShadeFlat, RenderShader.ShadeFlat);
            go.id = SceneManager.gameObjects.Count;
            go.name = "GameObject #" + go.id.ToString();
            SceneManager.gameObjects.Add(go);
            // make hierarchy object
            GUIButton hierarchyButtonObject = new GUIButton(new Vector3(39, -3.5f * hierarchy.contents.Count - 35f, 0), new Vector2(-1f, -1f), new Vector2(0.38f, 0.03f));
            hierarchyButtonObject.visible = false;
            hierarchyButtonObject.renderOrder = -1;
            hierarchyButtonObject.name = go.id.ToString() + " button object";
            hierarchyButtonObject.buttonDown += SelecthierarchyObject;
            GUIImage hierarchyImageObject = new GUIImage(Vector3.Zero, Vector2.Zero, new Vector2(0.38f, 0.03f), new Vector4(0, 0, 0, 100));
            hierarchyImageObject.parentGUI = hierarchyButtonObject;
            hierarchyImageObject.name = go.id.ToString() + " image object";
            GUIText hierarchyTextObject = new GUIText(Vector3.Zero, Vector2.Zero, 0.07f, go.name, @"C:\Windows\Fonts\Arial.ttf", 200f, new Vector4(0, 0, 0, 0), new Vector4(200, 200, 200, 255));
            hierarchyTextObject.name = go.id.ToString() + " text object";
            hierarchyTextObject.parentGUI = hierarchyButtonObject;
            hierarchyTextObject.localRenderOrder = 1;
            GUIManager.AddGUIElement(hierarchyTextObject);
            hierarchy.contents.Add(hierarchyButtonObject);
            GUIManager.AddGUIElement(hierarchyImageObject);
            /*
            GUIManager.hierarchyQueue.Add(hierarchyTextObject);
            GUIManager.hierarchyQueue.Add(hierarchyButtonObject);
            GUIManager.hierarchyQueue.Add(hierarchyImageObject);
            */
        }

        public static void XPButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(1f, 0, 0) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position += move * deltaTime;
            }
        }

        public static void XMButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(1f, 0, 0) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position -= move * deltaTime;
            }
        }

        public static void YPButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(0, 1f, 0) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position += move * deltaTime;
            }
        }

        public static void YMButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(0, 1f, 0) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position -= move * deltaTime;
            }
        }

        public static void ZPButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(0, 0, 1f) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position -= move * deltaTime;
            }
        }

        public static void ZMButtonHold(float deltaTime, string buttonName)
        {
            float speed = 5;
            Vector3 move = new Vector3(0, 0, 1f) * speed;
            if (GUIManager.selectedhierarchyObject != null)
            {
                GameObject go = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedhierarchyObject.name.Replace(" button object", "")));
                go.position += move * deltaTime;
            }
        }

        public static void SelecthierarchyObject(string buttonName)
        {
            GUIElement prevSelectedObject = GUIManager.selectedhierarchyObject;

            // unselect old
            if (GUIManager.selectedhierarchyObject != null)
            {
                foreach (GUIElement element in GUIManager.guiElements)
                {
                    if (element.name == GUIManager.selectedhierarchyObject.name.Replace(" button object", " image object"))
                    {
                        GUIImage _element = (GUIImage)element;
                        _element.SetColour(new Vector4(0, 0, 0, 100));
                    }
                }
            }

            // select new
            GUIElement selectedButton = GUIManager.GetElementByName(buttonName);
            GUIManager.selectedhierarchyObject = selectedButton;
            if (prevSelectedObject != GUIManager.selectedhierarchyObject)
            {
                foreach (GUIElement element in GUIManager.guiElements) // make blue
                {
                    if (element.name == buttonName.Replace(" button object", " image object"))
                    {
                        GUIImage _element = (GUIImage)element;
                        _element.SetColour(new Vector4(50, 100, 255, 255));
                    }
                }
            }
            else
            {
                GUIManager.selectedhierarchyObject = null;
                foreach (GUIElement element in GUIManager.guiElements) // make black (unselect)
                {
                    if (element.name == buttonName.Replace(" button object", " image object"))
                    {
                        GUIImage _element = (GUIImage)element;
                        _element.SetColour(new Vector4(0, 0, 0, 100));
                    }
                }
            }

            GUIManager.RefreshInspectorMenu();
        }
    }
}