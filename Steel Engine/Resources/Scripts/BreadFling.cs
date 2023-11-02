using OpenTK.Mathematics;
using Steel_Engine;
using Steel_Engine.Common;
using Steel_Engine.GUI;
using System;

public class BreadFling : Component
{
    private Vector3 prevPos = Vector3.Zero;
    private Vector3 velocity = Vector3.Zero;
    private GUIText positionText;
    private float maxHeight;
    private bool started;
    private float gravity = 0.01f;

    public override void Tick(float deltaTime)
    {
        if (gameObject.position.Y > maxHeight)
            maxHeight = gameObject.position.Y;
        if (started && gameObject.position.Y < maxHeight)
        {
            // falling
            positionText = new GUIText(new Vector3(0, 700, 0), new Vector2(0, -1), 5, "Max height: " + maxHeight.ToString(), "C:\\Windows\\Fonts\\Arial.ttf", 20);
            GUIManager.AddGUIElement(positionText);
        }
        if (InputManager.GetMouseButton(0))
        {
            Vector3 temp;
            SteelRay ray = SceneManager.CalculateRay(InputManager.mousePosition);
            Vector3 pos = MathFExtentions.LinePlaneIntersection(new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0), ray.worldPosition, ray.worldDirection);
            temp = gameObject.position;
            gameObject.position = pos;
            prevPos = temp;
        }

        if (InputManager.GetMouseButtonUp(0))
        {
            Vector3 diff = (gameObject.position - prevPos);
            velocity = diff;
            started = true;
        }

        if (!InputManager.GetMouseButton(0))
        {
            gameObject.position += velocity;
            if (started)
            {
                velocity -= new Vector3(0, 1, 0) * gravity;
            }
            //SceneManager.cameras[0].Position = new Vector3(gameObject.position.X, gameObject.position.Y, SceneManager.cameras[0].Position.Z);
        }
    }
}