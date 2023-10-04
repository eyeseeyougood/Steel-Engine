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
    public class ImageReference
    {
        public string name;
        public string extention;

        public ImageReference(string name, string extention)
        {
            this.name = name;
            this.extention = extention;
        }
    }

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

    public class GUIElementComparer : IComparer<GUIElement>
    {
        public int Compare(GUIElement a, GUIElement b)
        {
            if (a.renderOrder <= b.renderOrder)
            {
                return -1;
            }
            else if (a.renderOrder > b.renderOrder)
            {
                return 1;
            }
            return -1;
        }
    }

    public class GUIElement
    {
        public int renderOrder = 0;
        public string name;
        public float zRotation;
        public Vector2 position; // arbitrary units
        public Vector3 addedPosition;
        public Vector2 anchor;
        public GUIElement parentGUI = null;
        public bool visible = true;

        public int textureId { get; private set; }
        public Bitmap texture { get; private set; }
        public GameObject renderObject { get; private set; }
        public List<string> textures { get; private set; }

        public GUIElement(Vector3 position, Vector2 anchor, RenderShader vShader, RenderShader fShader)
        {
            addedPosition = position;
            this.anchor = anchor;
            
            textures = new List<string>();

            renderObject = new GameObject(vShader, fShader);
            renderObject.mesh = OBJImporter.LoadOBJFromPath(InfoManager.currentDir + @$"\EngineResources\EngineModels\Quad.obj", true);
            renderObject.Load();
        }

        public void SetZRotation(float rot) // not the fastest of functions because the whole objects gets reloaded every time
        {
            rot = MathHelper.DegreesToRadians(-rot);
            zRotation = rot;
            renderObject.Load(Quaternion.FromEulerAngles(new Vector3(0, 0, rot)));
        }

        public virtual void Tick(float deltaTime, params object[] args)
        {
            if (parentGUI != null)
            {
                addedPosition = parentGUI.addedPosition;
                anchor = parentGUI.anchor;
                renderOrder = parentGUI.renderOrder+1;
            }
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
            if (visible)
            {
                renderObject.Render();
            }
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
        public delegate void GUIButtonDown();
        public event GUIButtonDown buttonDown = new GUIButtonDown(_ButtonDown);
        public delegate void GUIButtonHold(float deltaTime);
        public event GUIButtonHold buttonHold = new GUIButtonHold(_ButtonHold);
        public delegate void GUIButtonUp();
        public event GUIButtonUp buttonUp = new GUIButtonUp(_ButtonUp);
        Vector2 scale;
        private Texture normalImage = null;
        private Texture pressedImage = null;
        private Texture currentImage = null;

        private static void _ButtonDown() { }
        private static void _ButtonHold(float deltaTime) { }
        private static void _ButtonUp() { }

        public void SetPressedImage(string name, string extention)
        {
            pressedImage = Texture.LoadFromFile(InfoManager.dataPath + @$"/Textures/{name}{extention}");
        }

        public override void Tick(float deltaTime, params object[] args)
        {
            Vector2 mousePosition = (Vector2)args[0];
            // args [0] is always a vector 2 of the mouse position
            MouseState mouseState = (MouseState)args[1];
            bool mouseDown = mouseState.IsButtonDown(MouseButton.Left);
            bool mousePressed = mouseState.IsButtonPressed(MouseButton.Left);
            bool mouseUp = mouseState.IsButtonReleased(MouseButton.Left);
            // args [1] is always the mouse state
            if (mousePressed)
            {
                if (CheckBounds(mouseState))
                {
                    buttonDown.Invoke();
                }
            }
            if (mouseUp)
            {
                if (CheckBounds(mouseState))
                {
                    buttonUp.Invoke();
                }
            }
            if (mouseDown)
            {
                if (CheckBounds(mouseState))
                {
                    buttonHold.Invoke(deltaTime);
                    if (pressedImage != null)
                    {
                        renderObject.LoadTexture(pressedImage);
                        currentImage = pressedImage;
                    }
                }
            }
            else
            {
                if (normalImage != null && currentImage != normalImage)
                {
                    renderObject.LoadTexture(normalImage);
                    currentImage = normalImage;
                }
            }

            base.Tick(deltaTime, args);
        }

        private bool CheckBounds(MouseState mouseState)
        {
            bool result = false;

            // Step 1: find line plane intersection point
            SteelRay ray = SceneManager.CalculateRay(mouseState);
            SteelTriangle triangle = renderObject.mesh.triangles[0];
            Vector3 p1 = triangle.GetVertex(0).position;
            p1 = renderObject.position + InfoManager.engineCamera.Right * p1.X * renderObject.scale.X + InfoManager.engineCamera.Up * p1.Y * renderObject.scale.Y;
            Vector3 p2 = triangle.GetVertex(1).position;            
            p2 = renderObject.position + InfoManager.engineCamera.Right * p2.X * renderObject.scale.X + InfoManager.engineCamera.Up * p2.Y * renderObject.scale.Y;
            Vector3 p3 = triangle.GetVertex(2).position;
            p3 = renderObject.position + InfoManager.engineCamera.Right * p3.X * renderObject.scale.X + InfoManager.engineCamera.Up * p3.Y * renderObject.scale.Y;
            Vector3 intersectionPoint = MathFExtentions.LinePlaneIntersection(p1, p2, p3, ray.worldPosition, ray.worldDirection);

            // Step 2: find two perpendicular vectors of quad and do dot product comparisons
            Vector3 bottomLeftUp = p2 - p3;
            Vector3 bottomLeftRight = p1 - p3;
            Vector3 intersectionPointDifference = intersectionPoint - p3;

            float verticalProduct = Vector3.Dot(intersectionPointDifference, bottomLeftUp.Normalized());
            float horizontalProduct = Vector3.Dot(intersectionPointDifference, bottomLeftRight.Normalized());

            // Step 3: check for dot product size and sign
            if (verticalProduct >= 0) // is positive
            {
                if (verticalProduct <= bottomLeftUp.Length) // might change to LengthFast if too slow
                {
                    // is within the height of quad
                    if (horizontalProduct >= 0)
                    {
                        if (horizontalProduct <= bottomLeftRight.Length)
                        {
                            // is within the width of the quad
                            // and so is within the quad it self
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            this.scale = scale;
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string textureExtention) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            normalImage = Texture.LoadFromFile(InfoManager.dataPath + @$"/Textures/{texture}{textureExtention}");
            renderObject.LoadTexture(normalImage);
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
        private Vector4 bgColour = Vector4.One;

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
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

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size, Vector4 bgColour) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            ApplyTexture(GUIManager.Write_Text(text, font, size, bgColour));

            this.bgColour = bgColour;

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

        public void PreloadText(string text)
        {
            string normalText = this.text;
            SetText(text);
            SetText(normalText);
        }

        public void SetText(string text)
        {
            this.text = text;
            if (!textures.Contains(text))
                textures.Add(text);

            if (!File.Exists(InfoManager.currentDir + @$"\Temp\{text}.png"))
            {
                ApplyTexture(GUIManager.Write_Text(text, font, size, bgColour));
                Rectangle dst = new Rectangle();
                dst.X = (int)((float)(rect.Width - texture.Width) / 2.0f);
                dst.Y = (int)((float)(rect.Height - texture.Height) / 2.0f);
                dst.Width = texture.Width;
                dst.Height = texture.Height;
                ApplyTexture(GUIManager.Write_Text(text, font, size, bgColour, dst, rect));
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

    public class GUIImage : GUIElement
    {
        Vector2 scale;
        public Texture image = null;

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector3 colour) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            renderObject.mesh.SetColour(colour);
            renderObject.Load();
            this.scale = scale;
        }

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector4 colour) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            Bitmap image = new Bitmap(1, 1);
            image.SetPixel(0, 0, Color.FromArgb((int)colour.W, (int)colour.X, (int)colour.Y, (int)colour.Z));
            image.Save(InfoManager.currentDir + @$"/Temp/{(colour)}");
            renderObject.LoadTexture(InfoManager.currentDir + @$"/Temp/{(colour)}");
            renderObject.Load();
            this.scale = scale;
        }

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string textureExtention) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            renderObject.LoadTexture(InfoManager.dataPath + @$"/Textures/{texture}{textureExtention}");
            image = Texture.LoadFromFile(InfoManager.dataPath + @$"/Textures/{texture}{textureExtention}");
            this.scale = scale;
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            base.Render();
        }
    }

    // EXPERIMENTAL CODE (NOT WORKING CORRECTLY)
    public class GUIWorldButton : GUIElement
    {
        public delegate void GUIButtonDown();
        public event GUIButtonDown buttonDown = new GUIButtonDown(_ButtonDown);
        public delegate void GUIButtonHold(float deltaTime);
        public event GUIButtonHold buttonHold = new GUIButtonHold(_ButtonHold);
        public delegate void GUIButtonUp();
        public event GUIButtonUp buttonUp = new GUIButtonUp(_ButtonUp);
        Vector2 scale;
        private Texture normalImage = null;
        private Texture pressedImage = null;
        private Texture currentImage = null;

        private static void _ButtonDown() { }
        private static void _ButtonHold(float deltaTime) { }
        private static void _ButtonUp() { }

        public void SetPressedImage(string name, string extention)
        {
            pressedImage = Texture.LoadFromFile(InfoManager.dataPath + @$"/Textures/{name}{extention}");
        }

        public override void Tick(float deltaTime, params object[] args)
        {
            Vector2 mousePosition = (Vector2)args[0];
            // args [0] is always a vector 2 of the mouse position
            MouseState mouseState = (MouseState)args[1];
            bool mouseDown = mouseState.IsButtonDown(MouseButton.Left);
            bool mousePressed = mouseState.IsButtonPressed(MouseButton.Left);
            bool mouseUp = mouseState.IsButtonReleased(MouseButton.Left);
            // args [1] is always the mouse state
            if (mousePressed)
            {
                if (CheckBounds(mouseState))
                {
                    buttonDown.Invoke();
                }
            }
            if (mouseUp)
            {
                if (CheckBounds(mouseState))
                {
                    buttonUp.Invoke();
                }
            }
            if (mouseDown)
            {
                if (CheckBounds(mouseState))
                {
                    buttonHold.Invoke(deltaTime);
                    if (pressedImage != null)
                    {
                        renderObject.LoadTexture(pressedImage);
                        currentImage = pressedImage;
                    }
                }
            }
            else
            {
                if (normalImage != null && currentImage != normalImage)
                {
                    renderObject.LoadTexture(normalImage);
                    currentImage = normalImage;
                }
            }

            base.Tick(deltaTime, args);
        }

        private bool CheckBounds(MouseState mouseState)
        {
            bool result = false;

            // Step 1: find line plane intersection point
            SteelRay ray = SceneManager.CalculateRay(mouseState);
            SteelTriangle triangle = renderObject.mesh.triangles[0];
            Vector3 p1 = triangle.GetVertex(0).position;
            p1 = renderObject.position + InfoManager.engineCamera.Right * p1.X * renderObject.scale.X + InfoManager.engineCamera.Up * p1.Y * renderObject.scale.Y + InfoManager.engineCamera.Front * p1.Z * renderObject.scale.Z;
            Vector3 p2 = triangle.GetVertex(1).position;
            p2 = renderObject.position + InfoManager.engineCamera.Right * p2.X * renderObject.scale.X + InfoManager.engineCamera.Up * p2.Y * renderObject.scale.Y + InfoManager.engineCamera.Front * p2.Z * renderObject.scale.Z;
            Vector3 p3 = triangle.GetVertex(2).position;
            p3 = renderObject.position + InfoManager.engineCamera.Right * p3.X * renderObject.scale.X + InfoManager.engineCamera.Up * p3.Y * renderObject.scale.Y + InfoManager.engineCamera.Front * p3.Z * renderObject.scale.Z;
            Vector3 intersectionPoint = MathFExtentions.LinePlaneIntersection(p1, p2, p3, ray.worldPosition, ray.worldDirection);

            // Step 2: find two perpendicular vectors of quad and do dot product comparisons
            Vector3 bottomLeftUp = p2 - p3;
            Vector3 bottomLeftRight = p1 - p3;
            Vector3 intersectionPointDifference = intersectionPoint - p3;

            float verticalProduct = Vector3.Dot(intersectionPointDifference, bottomLeftUp.Normalized());
            float horizontalProduct = Vector3.Dot(intersectionPointDifference, bottomLeftRight.Normalized());

            // Step 3: check for dot product size and sign
            if (verticalProduct >= 0) // is positive
            {
                if (verticalProduct <= bottomLeftUp.Length) // might change to LengthFast if too slow
                {
                    // is within the height of quad
                    if (horizontalProduct >= 0)
                    {
                        if (horizontalProduct <= bottomLeftRight.Length)
                        {
                            // is within the width of the quad
                            // and so is within the quad it self
                            result = true;
                        }
                    }
                }
            }

            return result;
        }

        public GUIWorldButton(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            this.scale = scale;
            position -= new Vector3(0, renderObject.position.Y/2.0f, 0);
        }

        public GUIWorldButton(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string textureExtention) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            renderObject.LoadTexture(InfoManager.dataPath + @$"/Textures/{texture}{textureExtention}");
            normalImage = Texture.LoadFromFile(InfoManager.dataPath + @$"/Textures/{texture}{textureExtention}");
            this.scale = scale;
            this.addedPosition += new Vector3(0, -renderObject.position.Y, 0);
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            renderObject.SetRotation(InfoManager.engineCamera.GetViewMatrix().ExtractRotation().Inverted());
            renderObject.Render();
        }
    }
}