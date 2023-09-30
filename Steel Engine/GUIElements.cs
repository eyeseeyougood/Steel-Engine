using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using static System.Formats.Asn1.AsnWriter;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading;

namespace Steel_Engine.GUI
{
    public class SteelRay
    {
        public Vector3 worldPosition;
        public Vector3 worldDirection;
        public float stepSize = 0.1f;
        public float distance = -1;
        public float threshhold = 0.01f;

        public SteelRay(Vector3 worldPosition, Vector3 worldDirection)
        {
            this.worldPosition = worldPosition;
            this.worldDirection = worldDirection;
        }
    }

    public class GUIElement
    {
        public Vector2 position; // arbitrary units
        public Vector3 addedPosition;
        public Vector2 anchor;
        public int textureId { get; private set; }
        public Bitmap texture { get; private set; }
        public GameObject renderObject { get; private set; }
        public List<string> textures { get; private set; }

        public GUIElement(Vector3 position, Vector2 anchor)
        {
            addedPosition = position;
            this.anchor = anchor;
            
            textures = new List<string>();

            renderObject = new GameObject(RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit);
            renderObject.LoadTexture(InfoManager.currentDir + @$"\EngineResources\EngineTextures\blank.png");
            renderObject.mesh = OBJImporter.LoadOBJFromPath(InfoManager.currentDir + @$"\EngineResources\EngineModels\Quad.obj", true);
            renderObject.Load();
        }

        public virtual void Tick(float deltaTime, params object[] args)
        {

        }

        public void ApplyTexture(Bitmap bmp)
        {
            texture = bmp;
        }

        public virtual void Render()
        {
            Vector3 camRight = InfoManager.engineCamera.Right;
            Vector3 camUp = InfoManager.engineCamera.Up;
            Vector2 remappedAnchor = Vector2.Zero;
            remappedAnchor = new Vector2(anchor.X * 0.933f, -anchor.Y * 0.522f * (InfoManager.windowSize.X / InfoManager.windowSize.Y));
            float hh = MathF.Tan(MathHelper.DegreesToRadians(InfoManager.engineCamera.Fov) / 2.0f) * 1;
            float aspect = InfoManager.windowSize.X / InfoManager.windowSize.Y;
            renderObject.position = InfoManager.engineCamera.Position + InfoManager.engineCamera.Front * 1
                + remappedAnchor.X * camRight * hh * aspect
                + remappedAnchor.Y * camUp * hh
                + camRight * addedPosition.X * 0.007f
                + camUp * addedPosition.Y * 0.007f;
            renderObject.SetRotation(InfoManager.engineCamera.GetViewMatrix().ExtractRotation().Inverted());
            renderObject.Render();
        }
        public virtual void CleanUp()
        {
            foreach (string _texture in textures)
            {
                File.Delete(InfoManager.currentDir + @$"\Temp\{_texture}.png");
            }
        }
    }

    public class GUIButton : GUIElement
    {
        public delegate void GUIButtonClick();
        public event GUIButtonClick buttonClicked;
        Vector2 scale;

        public override void Tick(float deltaTime, params object[] args)
        {
            Vector2 mousePosition = (Vector2)args[0];
            // args [0] is always a vector 2 of the mouse position for the Button Object
            bool mouseDown = ((MouseState)args[1]).IsButtonPressed(MouseButton.Left);
            // args [1] is always a boolean of the mouse state (up or down) for the Button Object
            if (mouseDown)
            {
                if (CheckBounds(mousePosition))
                {
                    Console.WriteLine("clicked " + mousePosition);
                }
            }
        }

        private bool CheckBounds(Vector3 pointOnPlane)
        {
            bool result = true;

            // step 1: find point on plane has already been calculated
            Vector3 screenPos1 = WorldToScreenspace(renderObject.position, renderObject.GetModelMatrix());

            Console.WriteLine(screenPos1);

            return result;
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor)
        {
            this.scale = scale;
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            base.Render();
        }
    }

    public class GUIText : GUIElement
    {
        public string text;
        public string font;
        public float size;
        public float scale;
        private Rectangle rect;
        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size) : base(position, anchor)
        {
            ApplyTexture(GUIManager.Write_Text(text, font, size));

            this.text = text;
            this.font = font;
            this.size = size;

            if (!textures.Contains(text))
                textures.Add(text);

            texture.Save(InfoManager.currentDir + @$"\Temp\{text}.png");

            this.scale = scale;

            rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = texture.Width;
            rect.Height = texture.Height;
            renderObject.LoadTexture(InfoManager.currentDir + @$"\Temp\{text}.png");
        }

        public void SetText(string text)
        {
            this.text = text;
            if (!textures.Contains(text))
                textures.Add(text);

            if (!File.Exists(InfoManager.currentDir + @$"\Temp\{text}.png"))
            {
                ApplyTexture(GUIManager.Write_Text(text, font, size));
                Rectangle dst = new Rectangle();
                dst.X = (int)((float)(rect.Width - texture.Width) / 2.0f);
                dst.Y = (int)((float)(rect.Height - texture.Height) / 2.0f);
                dst.Width = texture.Width;
                dst.Height = texture.Height;
                ApplyTexture(GUIManager.Write_Text(text, font, size, dst, rect));
                texture.Save(InfoManager.currentDir + @$"\Temp\{text}.png");
            }

            renderObject.LoadTexture(InfoManager.currentDir + @$"\Temp\{text}.png");
            renderObject.mesh = OBJImporter.LoadOBJFromPath(InfoManager.currentDir + @$"\EngineResources\EngineModels\Quad.obj", true);
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(0.07f * (float)texture.Width, 0.07f * (float)texture.Height, 1f) * 0.025f * scale;
            base.Render();
        }
    }
}