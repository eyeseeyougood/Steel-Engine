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

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            InfoManager.windowSize = Size;
            InfoManager.engineCamera = new Camera(Vector3.UnitZ * 3, Size.X / Size.Y);
            InfoManager.engineCamera.Fov = 90;

            CursorState = CursorState.Grabbed;

            // TEST CODE
            InfoManager.testSphere = new GameObject(RenderShader.ShadeFlat, RenderShader.ShadeFlat);
            InfoManager.testSphere.scale = Vector3.One * 0.01f;
            InfoManager.testSphere.mesh = OBJImporter.LoadOBJ("Sphere", true);
            InfoManager.testSphere.mesh.SetColour(new Vector3(1, 1, 1));
            InfoManager.testSphere.Load();

            // set build mode
            InfoManager.isBuild = bool.Parse(File.ReadAllLines(InfoManager.currentDir + @"/BuildSettings/BuildSettings.txt")[0].Replace("isBuild ", ""));

            // load scene 0
            SceneManager.Init();
            SceneManager.LoadScene(0);

            // load ui
            if (!InfoManager.isBuild)
                GUIManager.LoadEngineGUI();

            // once loaded, if is a build make it always running
            if (InfoManager.isBuild)
                SceneManager.gameRunning = true;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (!IsFocused) { return; }

            InputManager.Tick(KeyboardState, MouseState, CursorState);

            if (InputManager.GetKeyDown(Keys.Escape))
            {
                CursorState = CursorState == CursorState.Normal ? CursorState.Grabbed : CursorState.Normal;
            }

            if (InputManager.GetKeyDown(Keys.R) && !InfoManager.isBuild)
            {
                SceneManager.gameRunning = !SceneManager.gameRunning;
                GUIText text = (GUIText)GUIManager.GetElementByName("topbarSimText");
                text.SetText(SceneManager.gameRunning ? "Simulating" : "Not Simulating");
            }

            GUIManager.Tick((float)args.Time);
            SceneManager.Tick(args.Time);
            LightManager.Tick();

            InfoManager.engineCamera.Tick(args.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            double deltaTime = args.Time;
            InfoManager.upTime += deltaTime;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            foreach (GameObject gameObject in SceneManager.gameObjects)
            {
                gameObject.Render();
            }

            GL.Disable(EnableCap.DepthTest);
            if (!InfoManager.isBuild)
                GizmoManager.RenderGizmos();
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
            InfoManager.windowSize = e.Size;
        }
    }
}