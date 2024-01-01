using OpenTK.Mathematics;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public class AxisMoveGizmo : Gizmo
    {
        private bool movingTestGizmo;

        public void Down(string buttonName)
        {
            movingTestGizmo = true;
        }

        public void Up(string buttonName)
        {
            movingTestGizmo = false;
        }

        public override void Tick(float deltaTime)
        {
            GUIWorldButton go = GetGUIElement<GUIWorldButton>();
            if (movingTestGizmo)
            {
                SteelRay ray = SceneManager.CalculateRay(InputManager.mousePosition);
                Vector3 intersectionPoint = MathFExtentions.FindClosestPointAxisAxis(ray.worldPosition, ray.worldDirection, go.addedPosition, Vector3.UnitX);
                Vector3 projectedPoint = new Vector3(-intersectionPoint.X, 0, 0);
                SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position.X = projectedPoint.X;
            }

            if (GUIManager.selectedHeirarchyObject != null)
            {
                go.addedPosition = SceneManager.GetGameObjectByID(int.Parse(GUIManager.selectedHeirarchyObject.name.Replace(" button object", ""))).position - Vector3.UnitX * 3;
                go.visible = true;
                go.active = true;
            }
            else
            {
                go.visible = false;
                go.active = false;
            }
        }

        public AxisMoveGizmo(Vector3 position, Vector3 scale, Vector3 rgb1, Vector3 axis) : base(new GUIWorldButton(position, scale, "Cube", rgb1))
        {
            GUIWorldButton gwb = GetGUIElement<GUIWorldButton>();
            gwb.SetModelFromPath(InfoManager.usingDirectory + @"\EngineResources\EngineModels\Arrow.Obj", true);
            gwb.buttonDown += Down;
            gwb.buttonUp += Up;
            if (axis != Vector3.UnitY && axis != -Vector3.UnitY)
            {
                Vector3 cross = Vector3.Cross(axis, Vector3.Cross(axis, Vector3.UnitY));
                int amount = 90;
                if (axis.X < 0 || axis.Y < 0 || axis.Z < 0)
                {
                    amount = -amount;
                }
                gwb.renderObject.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(90)).ToEulerAngles());
                gwb.renderObject.Rotate(Quaternion.FromAxisAngle(cross, MathHelper.DegreesToRadians(amount)).ToEulerAngles());
            }
            else
            {
                int amount = 90;
                if (axis.X < 0 || axis.Y < 0 || axis.Z < 0)
                {
                    amount = -amount;
                }
                gwb.renderObject.Rotate(Quaternion.FromAxisAngle(Vector3.UnitX, MathHelper.DegreesToRadians(amount)).ToEulerAngles());
            }
        }
    }
}