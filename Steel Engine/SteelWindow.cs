using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Steel_Engine.Common;
using Steel_Engine.GUI;

namespace Steel_Engine
{
    internal class SteelWindow : GameWindow
    {
        private bool firstMove = true;

        private Vector2 lastPos;

        private double upTime;

        private Stopwatch timer;

        private Vector2 originalSize;

        public SteelWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            GUIManager.Cleanup();

            base.OnClosing(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            originalSize = Size;
            InfoManager.windowSize = Size;
            InfoManager.engineCamera = new Camera(Vector3.UnitZ * 3, Size.X / Size.Y);

            CursorState = CursorState.Grabbed;

            // load scene 0
            SceneManager.Init();
            SceneManager.LoadScene(0);

            // load text
            GUIManager.AddGUIElement(new GUIText(new Vector3(0, -3.5f, 0), new Vector2(0f, -1f), 0.07f, "Not Simulating", @"C:\Windows\Fonts\Arial.ttf", 300f));
            GUIManager.AddGUIElement(new GUIButton(new Vector3(0, -5f, 0), new Vector2(-0.5f, -1f), new Vector2(0.3f, 0.05f)));
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (!IsFocused) { return; }

            var input = KeyboardState;

            SceneManager.gameObjects[1].position = LightManager.lights[0].position;

            if (input.IsKeyPressed(Keys.Escape))
            {
                CursorState = CursorState == CursorState.Normal ? CursorState.Grabbed : CursorState.Normal;
            }

            if (input.IsKeyPressed(Keys.R))
            {
                SceneManager.gameRunning = !SceneManager.gameRunning;
                GUIText text = (GUIText)GUIManager.GetElementByID(0);
                text.SetText(SceneManager.gameRunning ? "Simulating" : "Not Simulating");
            }

            // Test Code --
            if (input.IsKeyDown(Keys.Left))
            {
                LightManager.lights[0].position += new Vector3(-1, 0, 0) * (float)args.Time;
            }

            if (input.IsKeyDown(Keys.Right))
            {
                LightManager.lights[0].position += new Vector3(1, 0, 0) * (float)args.Time;
            }

            if (input.IsKeyDown(Keys.Up))
            {
                LightManager.lights[0].position += new Vector3(0, 1, 0) * (float)args.Time;
            }

            if (input.IsKeyDown(Keys.Down))
            {
                LightManager.lights[0].position += new Vector3(0, -1, 0) * (float)args.Time;
            }
            // ------

            GUIManager.Tick((float)args.Time, MousePosition, MouseState);
            SceneManager.Tick(args.Time);

            InfoManager.engineCamera.Tick(input, args.Time, CursorState, MouseState);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            upTime += args.Time; // deltaTime time is args.Time
            double deltaTime = args.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            foreach (GameObject gameObject in SceneManager.gameObjects)
            {
                gameObject.Render();
            }

            GL.Disable(EnableCap.DepthTest);
            GUIManager.Render();

            SwapBuffers();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            InfoManager.engineCamera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, e.Size.X, e.Size.Y);
            InfoManager.engineCamera.Fov = MathHelper.RadiansToDegrees(2.0f * MathF.Atan(((float)e.Size.Y * 0.5f) * ((float)e.Size.X/(float)e.Size.Y)));
            InfoManager.windowSize = e.Size;
        }
    }
}