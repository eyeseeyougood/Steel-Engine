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

namespace Steel_Engine.GUI
{
    public abstract class GUIElement
    {
        public abstract void Render();
        public abstract void CleanUp();
    }

    public class GUIText : GUIElement
    {
        public Vector2 position; // screen space position
        public Vector3 addedPosition;
        public string text;
        public string font;
        public float size;
        public float scale;
        public Vector2 anchor;
        private int textureId;
        private Bitmap texture;
        public GameObject renderObject { get; private set; }
        private Rectangle rect;
        private List<string> textures = new List<string>();

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size)
        {
            texture = GUIManager.Write_Text(text, font, size);

            this.text = text;
            this.font = font;
            this.size = size;
            this.anchor = anchor;
            this.addedPosition = position;

            if (!textures.Contains(text))
                textures.Add(text);

            texture.Save(InfoManager.currentDir + @$"\Temp\{text}.png");

            renderObject = new GameObject(RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit);

            this.scale = scale;

            rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = texture.Width;
            rect.Height = texture.Height;
            renderObject.LoadTexture(InfoManager.currentDir + @$"\Temp\{text}.png");
            renderObject.mesh = OBJImporter.LoadOBJFromPath(InfoManager.currentDir + @$"\EngineResources\EngineModels\Quad.obj", true);
            renderObject.Load();
        }

        public void SetText(string text)
        {
            this.text = text;
            if (!textures.Contains(text))
                textures.Add(text);

            if (!File.Exists(InfoManager.currentDir + @$"\Temp\{text}.png"))
            {
                texture = GUIManager.Write_Text(text, font, size);
                Rectangle dst = new Rectangle();
                dst.X = (int)((float)(rect.Width - texture.Width) / 2.0f);
                dst.Y = (int)((float)(rect.Height - texture.Height) / 2.0f);
                dst.Width = texture.Width;
                dst.Height = texture.Height;
                texture = GUIManager.Write_Text(text, font, size, dst, rect);
                texture.Save(InfoManager.currentDir + @$"\Temp\{text}.png");
            }

            renderObject.LoadTexture(InfoManager.currentDir + @$"\Temp\{text}.png");
            renderObject.mesh = OBJImporter.LoadOBJFromPath(InfoManager.currentDir + @$"\EngineResources\EngineModels\Quad.obj", true);
        }

        public override void CleanUp()
        {
            foreach (string _texture in textures)
            {
                File.Delete(InfoManager.currentDir + @$"\Temp\{_texture}.png");
            }
        }

        public override void Render()
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
            renderObject.scale = new Vector3(0.07f * (float)texture.Width, 0.07f * (float)texture.Height, 1f) * 0.025f * scale;
            renderObject.SetRotation(InfoManager.engineCamera.GetViewMatrix().ExtractRotation().Inverted());
            renderObject.Render();
        }
    }
}