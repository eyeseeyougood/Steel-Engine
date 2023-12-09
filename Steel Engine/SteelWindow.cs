﻿using System;
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
using Steel_Engine.Tilemaps;
using Zamak;

namespace Steel_Engine
{
    internal class SteelWindow : GameWindow
    {
        public SteelWindow(GameWindowSettings settings, NativeWindowSettings nativeSettings) : base(settings, nativeSettings)
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            MultiplayerManager.CleanUp();
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

            InfoManager.Tick(0);

            CursorState = CursorState.Grabbed;

            // Set events
            InfoManager.setWindowSize += SetWindowSize;
            InfoManager.setWindowState += SetWindowState;
            InfoManager.setWindowTitle += SetWindowTitle;
            InfoManager.setWindowPosition += SetWindowPosition;

            // Test Code
            GameObject testObject = new GameObject(RenderShader.ShadeFlat, RenderShader.ShadeFlat);
            testObject.scale = Vector3.One * 0.05f;
            testObject.mesh = OBJImporter.LoadOBJ("Cube", true);
            testObject.mesh.SetColour(Vector3.UnitX);
            testObject.Load();
            InfoManager.testObject = testObject;

            // set build mode
            InfoManager.isBuild = bool.Parse(File.ReadAllLines(InfoManager.currentDevPath + @"/BuildSettings/BuildSettings.txt")[0].Replace("isBuild ", ""));
            BuildManager.buildClientServerFiles = bool.Parse(File.ReadAllLines(InfoManager.currentDevPath + @"/BuildSettings/BuildSettings.txt")[2].Replace("isMultiplayer ", ""));

            // if is build then build all required files
            if (!File.Exists(InfoManager.currentDir + @"\Runtimes\Xq65.txt"))
            {
                BuildManager.AssembleFiles();
                if (!bool.Parse(File.ReadAllLines(InfoManager.currentDevPath + @"/BuildSettings/BuildSettings.txt")[1].Replace("testingBuild ", "")))
                {
                    BuildManager.Lock();
                }
            }

            // load scene 0
            SceneManager.Init();
            SceneManager.LoadScene(0);

            // load ui
            if (!InfoManager.isBuild)
                GUIManager.LoadEngineGUI();

            // once loaded, if is a build make it always running
            if (InfoManager.isBuild)
                SceneManager.gameRunning = true;

            // multiplayer
            if (InfoManager.executingArgs.Length > 0)
            {
                MultiplayerManager.isMultiplayer = InfoManager.executingArgs[0] == "/M";
            }
            if (MultiplayerManager.isMultiplayer)
            {
                string[] multiplayerArgs = InfoManager.executingArgs.Skip(1).ToArray();
                MultiplayerManager.Init(multiplayerArgs);
            }
        }

        public void SetWindowSize(int x, int y)
        {
            ClientSize = new Vector2i(x, y);
        }

        public void SetWindowState(WindowState state)
        {
            WindowState = state;
        }

        public void SetWindowTitle(string title)
        {
            Title = title;
        }

        public void SetWindowPosition(Vector2i position)
        {
            ClientLocation = position;
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
                text.SetText(SceneManager.gameRunning ? 1 : 0);
            }

            InfoManager.Tick((float)args.Time);
            GUIManager.Tick((float)args.Time);
            SceneManager.Tick(args.Time);
            LightManager.Tick();
            MultiplayerManager.Tick((float)args.Time);

            InfoManager.engineCamera.Tick(args.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            double deltaTime = args.Time;
            Time.upTime += deltaTime;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            foreach (GameObject gameObject in SceneManager.gameObjects)
            {
                gameObject.Render();
            }

            foreach (Tilemap tilemap in SceneManager.tilemaps)
            {
                tilemap.Render();
            }

            GL.Disable(EnableCap.DepthTest);
            if (!InfoManager.isBuild)
                GizmoManager.RenderGizmos();
            GUIManager.Render();

            SwapBuffers();
        }

        protected override void OnMove(WindowPositionEventArgs e)
        {
            base.OnMove(e);
            InfoManager.windowPosition = ClientLocation;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            
            GL.Viewport(0, 0, e.Size.X, e.Size.Y);
            InfoManager.windowSize = e.Size;
        }
    }
}