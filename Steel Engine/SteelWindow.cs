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
        private Camera camera;

        private bool firstMove = true;

        private Vector2 lastPos;

        private double upTime;

        private Stopwatch timer;

        private List<GUIElement> guiElements = new List<GUIElement>();

        public SteelWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (GUIElement element in guiElements)
            {
                element.CleanUp();
            }

            base.OnClosing(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);

            GL.Enable(EnableCap.DepthTest);

            camera = new Camera(Vector3.UnitZ * 3, Size.X / Size.Y);

            InfoManager.engineCamera = camera;

            CursorState = CursorState.Grabbed;

            // load scene 0
            SceneManager.Init();
            SceneManager.LoadScene(0);

            // load text
            guiElements.Add(new GUIText(new Vector2(0, 0), 2f, "Not Simulating", @"C:\Windows\Fonts\Arial.ttf", 21f));
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
                GUIText text = (GUIText)guiElements[0];
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

            SceneManager.Tick(args.Time);

            camera.Tick(input, args.Time, CursorState, MouseState);
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
            foreach (GUIElement guiElement in guiElements)
            {
                guiElement.Render();
            }

            SwapBuffers();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            camera.AspectRatio = Size.X / Size.Y;
            InfoManager.windowSize = Size;
        }
    }
}