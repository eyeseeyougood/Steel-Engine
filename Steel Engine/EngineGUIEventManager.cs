using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using Steel_Engine.GUI;

namespace Steel_Engine
{
    public class EngineGUIEventManager
    {
        public static void XPButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(1f, 0, 0) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position += move * deltaTime;
        }

        public static void XMButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(1f, 0, 0) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position -= move * deltaTime;
        }

        public static void YPButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(0, 1f, 0) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position += move * deltaTime;
        }

        public static void YMButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(0, 1f, 0) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position -= move * deltaTime;
        }

        public static void ZPButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(0, 0, 1f) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position -= move * deltaTime;
        }

        public static void ZMButtonHold(float deltaTime, string buttonName, params object[] args)
        {
            float speed = 1;
            Vector3 move = new Vector3(0, 0, 1f) * speed;
            if (GUIManager.selectedHeirarchyObject != null)
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object",""))).position += move * deltaTime;
        }

        public static void SelectHeirarchyObject(string buttonName, params object[] args)
        {
            GUIElement prevSelectedObject = GUIManager.selectedHeirarchyObject;

            // unselect old
            if (GUIManager.selectedHeirarchyObject != null)
            {
                foreach (GUIElement element in GUIManager.guiElements)
                {
                    if (element.name == GUIManager.selectedHeirarchyObject.name.Replace(" button object", " image object"))
                    {
                        GUIImage _element = (GUIImage)element;
                        _element.SetColour(new Vector4(0, 0, 0, 100));
                    }
                }
            }

            // select new
            GUIElement selectedButton = null;
            foreach (GUIElement element in GUIManager.guiElements)
            {
                if (element.name == buttonName)
                {
                    selectedButton = element;
                }
            }
            GUIManager.selectedHeirarchyObject = selectedButton;
            if (prevSelectedObject != GUIManager.selectedHeirarchyObject)
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
                GUIManager.selectedHeirarchyObject = null;
                foreach (GUIElement element in GUIManager.guiElements) // make black (unselect)
                {
                    if (element.name == buttonName.Replace(" button object", " image object"))
                    {
                        GUIImage _element = (GUIImage)element;
                        _element.SetColour(new Vector4(0, 0, 0, 100));
                    }
                }
            }
        }
    }
}