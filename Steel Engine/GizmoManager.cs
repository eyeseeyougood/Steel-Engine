using OpenTK.Mathematics;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class Gizmo
    {
        private GUIElement gizmoGUI;

        public Gizmo(GUIElement element)
        {
            gizmoGUI = element;
        }

        protected T GetGUIElement<T>() where T : GUIElement
        {
            return (T)Convert.ChangeType(gizmoGUI, gizmoGUI.GetType());
        }

        public string GetName()
        {
            return gizmoGUI.name;
        }

        public void SetName(string name)
        {
            gizmoGUI.name = name;
        }

        public void Render()
        {
            gizmoGUI.Render();
        }

        public virtual void Tick(float deltaTime)
        {
            gizmoGUI.Tick(deltaTime);
        }
    }

    public static class GizmoManager
    {
        public static List<Gizmo> gizmos = new List<Gizmo>();

        public static void Init()
        {
            if (InfoManager.isBuild)
                return;

            // create gizmos
            // TESTCODE
            Gizmo testButtonGizmo = new AxisMoveGizmo(new Vector3(0, 0, 0), new Vector3(0.25f, 0.25f, 0.25f), new Vector3(0, 0, 1), new Vector3(0, 0, 1));
            AddGizmo(testButtonGizmo);
        }

        public static void RenderGizmos()
        {
            LightManager.RenderGizmos();
            foreach (Gizmo gizmo in gizmos)
            {
                gizmo.Render();
            }
        }

        public static void Tick(float deltaTime)
        {
            foreach(Gizmo gizmo in gizmos)
            {
                gizmo.Tick(deltaTime);
            }
        }

        public static void AddGizmo(Gizmo gizmo)
        {
            gizmos.Add(gizmo);
        }
    }
}
