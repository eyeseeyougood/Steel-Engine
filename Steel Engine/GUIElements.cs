using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;

namespace Steel_Engine.GUI
{
    public class ImageReference
    {
        public string path;

        public ImageReference(string name, string extension)
        {
            this.path = InfoManager.usingDataPath + @$"\Textures\{name}{extension}";
        }

        public ImageReference(string path)
        {
            this.path = path;
        }
    }

    public class SteelRay
    {
        public Vector3 worldPosition;
        public Vector3 worldDirection;
        public float stepSize = 0.1f;
        public float distance = -1;
        public float threshold = 0.01f;

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
            if (a.renderOrder < b.renderOrder)
            {
                return -1;
            }
            else if (a.renderOrder > b.renderOrder)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }

    public class GUIElement
    {
        public int renderOrder = 0;
        public int localRenderOrder = 0;
        public string name;
        public float zRotation;
        public Vector3 addedPosition;
        public Vector2 anchor;
        public Vector2 size;
        public GUIElement parentGUI = null;
        public bool visible = true;
        public bool active = true;
        public bool useSize;
        private Vector3 initialAddedPosition;
        private Vector2 initialAnchor;

        public delegate void RenderingObject(GUIElement self);
        public event RenderingObject onRender = new RenderingObject(ORO);
        public event RenderingObject onRendered = new RenderingObject(ORO);

        public GameObject renderObject { get; private set; }
        public List<Texture> textures { get; private set; }
        public Texture currentTexture { get; private set; }
        public int texID { get; private set; }

        private static void ORO(GUIElement self) { }

        public GUIElement(Vector3 position, Vector2 anchor, RenderShader vShader, RenderShader fShader)
        {
            addedPosition = position;
            this.anchor = anchor;
            initialAddedPosition = position;
            initialAnchor = anchor;
            
            textures = new List<Texture>();

            string path = InfoManager.usingDirectory + @$"\EngineResources\EngineModels\Quad.obj";

            renderObject = new GameObject(vShader, fShader);
            renderObject.mesh = OBJImporter.LoadOBJFromPath(path, true);
            renderObject.Load();

            GUIManager.guiTick += Tick;
        }

        public void SetZRotation(float rot) // not the fastest of functions because the whole objects gets reloaded every time
        {
            rot = MathHelper.DegreesToRadians(-rot);
            zRotation = rot;
            renderObject.Load(Quaternion.FromEulerAngles(new Vector3(0, 0, rot)));
        }

        public virtual void Tick(float deltaTime)
        {
            UpdateParentLocalisations();
        }

        public void UpdateParentLocalisations()
        {
            if (parentGUI != null)
            {
                addedPosition = parentGUI.addedPosition + initialAddedPosition;
                anchor = parentGUI.anchor + new Vector2(parentGUI.size.X * initialAnchor.X, parentGUI.size.Y * initialAnchor.Y);
                renderOrder = parentGUI.renderOrder + 1 + localRenderOrder;
                active = parentGUI.active;
            }
        }

        public void ApplyTexture(Texture texture)
        {
            renderObject.LoadTexture(texture);
            currentTexture = texture;
            texID = textures.IndexOf(texture);
        }

        public virtual void Render()
        {
            // update parent localisations

            UpdateParentLocalisations();

            onRender.Invoke(this);

            // rendering
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
            if (useSize)
            {
                renderObject.scale = new Vector3(size.X, size.Y, 1f);
            }
            if (visible)
            {
                if ((parentGUI != null && parentGUI.active) || (parentGUI == null))
                {
                    renderObject.Render();
                    onRendered.Invoke(this);
                }
            }
        }
    }

    public class GUIButton : GUIElement
    {
        public delegate void GUIButtonDown(string buttonName);
        public event GUIButtonDown buttonDown = new GUIButtonDown(_ButtonDown);
        public delegate void GUIButtonHold(float deltaTime, string buttonName);
        public event GUIButtonHold buttonHold = new GUIButtonHold(_ButtonHold);
        public delegate void GUIButtonUp(string buttonName);
        public event GUIButtonUp buttonUp = new GUIButtonUp(_ButtonUp);
        private Texture normalImage = null;
        private Texture pressedImage = null;
        private Texture currentImage = null;

        private static void _ButtonDown(string buttonName) { }
        private static void _ButtonHold(float deltaTime, string buttonName) { }
        private static void _ButtonUp(string buttonName) { }

        public void SetPressedImage(string name, string extension)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{name}{extension}";

            pressedImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public void SetPressedImage(string path)
        {
            pressedImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public override void Tick(float deltaTime)
        {
            Vector2 mousePosition = InputManager.mousePosition;
            bool mousePressed = InputManager.GetMouseButtonDown(MouseButton.Left);
            bool mouseDown = InputManager.GetMouseButton(MouseButton.Left);
            bool mouseUp = InputManager.GetMouseButtonUp(MouseButton.Left);
            if (mousePressed)
            {
                if (CheckBounds(mousePosition) && (active) && IsTopButton())
                {
                    buttonDown.Invoke(name);
                }
            }
            if (mouseUp)
            {
                if (CheckBounds(mousePosition) && (active) && IsTopButton())
                {
                    buttonUp.Invoke(name);
                }
            }
            if (mouseDown)
            {
                if (CheckBounds(mousePosition) && (active) && IsTopButton())
                {
                    buttonHold.Invoke(deltaTime, name);
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

            base.Tick(deltaTime);
        }

        private bool CheckBounds(Vector2 mousePosition)
        {
            bool result = false;

            // Step 1: find line plane intersection point
            SteelRay ray = SceneManager.CalculateRay(mousePosition);
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

        public bool IsTopButton() // a bit of a slower function
        {
            bool result = true;

            foreach (GUIElement element in GUIManager.guiElements)
            {
                if (element.GetType() == typeof(GUIButton) && element.active)
                {
                    if (((GUIButton)element).CheckBounds(InputManager.mousePosition))
                    {
                        if (!CheckButtonHeight((GUIButton)element))
                        {
                            result = false;
                            break;
                        }
                    }
                }
                else if (element.GetType() == typeof(GUIScrollView) && element.active)
                {
                    foreach (GUIElement _element in ((GUIScrollView)element).contents)
                    {
                        if (element.GetType() == typeof(GUIButton) && element.active)
                        {
                            if (!CheckButtonHeight((GUIButton)_element))
                            {
                                result = false;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        private bool CheckButtonHeight(GUIButton button)
        {
            bool result = true;

            if (button.CheckBounds(InputManager.mousePosition))
            {
                if (button.renderOrder > renderOrder)
                {
                    result = false;
                }
            }

            return result;
        }

        public void SetColour(Vector3 rgb1)
        {
            renderObject.mesh.SetColour(rgb1);
            renderObject.Load();
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            this.size = scale;
            useSize = true;
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string extension) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{texture}{extension}";

            normalImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            renderObject.LoadTexture(normalImage);
            this.size = scale;
            useSize = true;
        }

        public GUIButton(Vector3 position, Vector2 anchor, Vector2 scale, string texturePath) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            normalImage = Texture.LoadFromFile(texturePath, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            renderObject.LoadTexture(normalImage);
            this.size = scale;
            useSize = true;
        }

    }

    public class GUIText : GUIElement
    {
        public string text;
        public string font;
        public float textSize;
        public float scale;
        private Rectangle rect;
        private Vector4 bgColour = Vector4.Zero;
        private Vector4 txtColour = Vector4.UnitW*255;
        private Vector2i dimentions = Vector2i.Zero;

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            Bitmap data = GUIManager.Write_Text(text, font, size);
            byte[] imageData = InfoManager.BitmapToByteArray(data);
            Texture texture = Texture.FromData(text, data.Width, data.Height, imageData);

            this.text = text;
            this.font = font;
            this.textSize = size;

            if (!textures.Contains(texture))
                textures.Add(texture);

            this.scale = scale;

            rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = data.Width;
            rect.Height = data.Height;

            dimentions = new Vector2i(data.Width, data.Height);

            ApplyTexture(texture);
        }

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size, Vector4 bg255) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            this.text = text;
            this.font = font;
            this.textSize = size;

            Bitmap data = GUIManager.Write_Text(text, font, size, bg255);
            byte[] imageData = InfoManager.BitmapToByteArray(data);
            Texture texture = Texture.FromData(text, data.Width, data.Height, imageData);

            this.bgColour = bg255;

            if (!textures.Contains(texture))
                textures.Add(texture);

            this.scale = scale;

            rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = data.Width;
            rect.Height = data.Height;

            dimentions = new Vector2i(data.Width, data.Height);

            ApplyTexture(texture);
        }

        public GUIText(Vector3 position, Vector2 anchor, float scale, string text, string font, float size, Vector4 bg255, Vector4 text255) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            Bitmap data = GUIManager.Write_Text(text, font, size, bg255, text255);
            byte[] imageData = InfoManager.BitmapToByteArray(data);
            Texture texture = Texture.FromData(text, data.Width, data.Height, imageData);

            this.bgColour = bg255;
            this.txtColour = text255;

            this.text = text;
            this.font = font;
            this.textSize = size;

            if (!textures.Contains(texture))
                textures.Add(texture);

            this.scale = scale;

            rect = new Rectangle();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = data.Width;
            rect.Height = data.Height;

            dimentions = new Vector2i(data.Width, data.Height);

            ApplyTexture(texture);
        }

        public void PreloadText(string text)
        {
            int normalText = texID;
            SetText(text);
            SetText(normalText);
        }

        public void SetText(int id)
        {
            ApplyTexture(textures[id]);
        }

        public void SetText(string text)
        {
            this.text = text;

            Bitmap data = GUIManager.Write_Text(text, font, textSize, bgColour, txtColour, rect, rect);
            byte[] imageData = InfoManager.BitmapToByteArray(data);
            Texture texture = Texture.FromData(text, data.Width, data.Height, imageData);

            if (!textures.Contains(texture))
                textures.Add(texture);

            dimentions = new Vector2i(data.Width, data.Height);

            ApplyTexture(texture);
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(0.07f * (float)dimentions.X, 0.07f * (float)dimentions.Y, 1f) * 0.025f * scale;
            base.Render();
        }
    }

    public class GUIImage : GUIElement
    {
        public Texture image = null;

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector3 rgb1) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            renderObject.mesh.SetColour(rgb1);
            renderObject.Load();
            size = scale;
            useSize = true;
        }

        public void SetColour(Vector4 rgb255)
        {
            Bitmap genImage = new Bitmap(1, 1);
            genImage.SetPixel(0, 0, Color.FromArgb((int)rgb255.W, (int)rgb255.X, (int)rgb255.Y, (int)rgb255.Z));
            byte[] imageData = InfoManager.BitmapToByteArray(genImage);
            Texture texture = Texture.FromData(rgb255.ToString(), genImage.Width, genImage.Height, imageData);

            if (!textures.Contains(texture))
                textures.Add(texture);

            image = texture;
            ApplyTexture(texture);
        }

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector4 rgb255) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            SetColour(rgb255);
            size = scale;
            useSize = true;
        }

        public GUIImage(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string extension) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{texture}{extension}";

            image = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            ApplyTexture(image);
            size = scale;
            useSize = true;
        }
    }

    public class GUIWorldImage : GUIElement
    {
        public Vector2 scale;
        public Texture image = null;

        public GUIWorldImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector3 rgb1) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            renderObject.mesh.SetColour(rgb1);
            renderObject.Load();
            this.scale = scale;
        }

        public GUIWorldImage(Vector3 position, Vector2 anchor, Vector2 scale, Vector4 rgb255) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            string path = InfoManager.usingDirectory + @$"\Temp\{rgb255}.png";

            Bitmap genImage = new Bitmap(1, 1);
            genImage.SetPixel(0, 0, Color.FromArgb((int)rgb255.W, (int)rgb255.X, (int)rgb255.Y, (int)rgb255.Z));
            genImage.Save(path);
            image = Texture.LoadFromFile(path);
            renderObject.LoadTexture(image);
            renderObject.Load();
            this.scale = scale;
        }

        public GUIWorldImage(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string extension) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{texture}{extension}";

            image = Texture.LoadFromFile(path);
            renderObject.LoadTexture(image);
            this.scale = scale;
        }

        public GUIWorldImage(Vector3 position, Vector2 anchor, Vector2 scale, string texturePath) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            image = Texture.LoadFromFile(texturePath, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            renderObject.LoadTexture(image);
            this.scale = scale;
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            renderObject.SetRotation(InfoManager.engineCamera.GetViewMatrix().ExtractRotation().Inverted());
            renderObject.position = addedPosition;
            renderObject.Render();
        }
    }

    public class GUIScrollView : GUIElement
    {
        private Vector3 colour;
        public float padding = 0.0f;
        public float spacing = 0.1f;
        public float scrollValue = 0;
        public float scrollStrength = 1;
        public List<GUIElement> contents = new List<GUIElement>();

        public bool CheckBounds(Vector2 mousePosition)
        {
            bool result = false;

            // Step 1: find line plane intersection point
            SteelRay ray = SceneManager.CalculateRay(mousePosition);
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

        public bool IsTopScrollView() // a bit of a slower function
        {
            bool result = true;

            foreach (GUIElement element in GUIManager.guiElements)
            {
                if (element.GetType() == typeof(GUIScrollView) && element.active)
                {
                    if (((GUIScrollView)element).CheckBounds(InputManager.mousePosition))
                    {
                        if (element.renderOrder > renderOrder)
                        {
                            result = false;
                            break;
                        }
                    }
                }
            }

            return result;
        }

        private void Init()
        {
            InputManager.onMouseWheelStateChanged += MouseScroll;
        }

        public GUIScrollView(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            size = scale;
            useSize = true;
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            Init();
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);
            foreach (GUIElement element in contents)
            {
                element.active = active;
            }
        }

        public void MouseScroll(Vector2i delta)
        {
            if (active && CheckBounds(InputManager.mousePosition) && IsTopScrollView())
                scrollValue += delta.Y;
        }

        public void SetColour(Vector3 rgb1)
        {
            renderObject.mesh.SetColour(rgb1);
            colour = rgb1;
            renderObject.Load();
        }

        private bool CheckVisiblity(GUIElement element)
        {
            bool result = true;
            if (element.anchor.Y + element.size.Y > anchor.Y + size.Y)
            {// is below the scroll view
                result = false;
            }
            if (element.anchor.Y - element.size.Y < anchor.Y - size.Y)
            {// is above the scroll view
                result = false;
            }
            return result;
        }

        public override void Render()
        {
            int index = 0;
            if (!visible)
                return;
            base.Render();
            if (scrollValue > 0)
            {
                scrollValue = 0;
            }
            GUIElement prevElement = null;
            foreach (GUIElement element in contents)
            {
                element.addedPosition = addedPosition;
                if (prevElement != null)
                {
                    element.anchor = prevElement.anchor - new Vector2(0, (-prevElement.size.Y) - (spacing) - element.size.Y);
                }
                else
                {
                    element.anchor = anchor - new Vector2(0, (size.Y-padding) - element.size.Y + 0.01f - (scrollValue * scrollStrength) / (15.5f + spacing));
                }
                bool visibility = CheckVisiblity(element);
                //visibility = true;
                element.visible = visibility;
                element.active = visibility;
                element.renderOrder = renderOrder + 1;
                index++;
                element.Render();
                prevElement = element;
            }
        }
    }

    // experimental (DO NOT USE) -- bodged together code
    public class GUIWorldButton : GUIElement
    {
        public Vector2 scale;
        public delegate void GUIButtonDown(string buttonName);
        public event GUIButtonDown buttonDown = new GUIButtonDown(_ButtonDown);
        public delegate void GUIButtonHold(float deltaTime, string buttonName);
        public event GUIButtonHold buttonHold = new GUIButtonHold(_ButtonHold);
        public delegate void GUIButtonUp(string buttonName);
        public event GUIButtonUp buttonUp = new GUIButtonUp(_ButtonUp);
        private Texture normalImage = null;
        private Texture pressedImage = null;
        private Texture currentImage = null;

        private static void _ButtonDown(string buttonName) { }
        private static void _ButtonHold(float deltaTime, string buttonName) { }
        private static void _ButtonUp(string buttonName) { }

        public void SetPressedImage(string name, string extension)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{name}{extension}";

            pressedImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public void SetPressedImage(string path)
        {
            pressedImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
        }

        public bool CheckBounds(Vector2 mousePosition)
        {
            bool result = false;

            // Step 1: find line plane intersection point
            SteelRay ray = SceneManager.CalculateRay(mousePosition);
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

        public GUIWorldButton(Vector3 position, Vector2 anchor, Vector2 scale) : base(position, anchor, RenderShader.ShadeFlat, RenderShader.ShadeFlat)
        {
            this.scale = scale;
        }

        public GUIWorldButton(Vector3 position, Vector2 anchor, Vector2 scale, string texture, string extension) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            string path = InfoManager.usingDataPath + @$"/Textures/{texture}{extension}";

            normalImage = Texture.LoadFromFile(path, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            renderObject.LoadTexture(normalImage);
            this.scale = scale;
        }

        public GUIWorldButton(Vector3 position, Vector2 anchor, Vector2 scale, string texturePath) : base(position, anchor, RenderShader.ShadeTextureUnit, RenderShader.ShadeTextureUnit)
        {
            normalImage = Texture.LoadFromFile(texturePath, TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            renderObject.LoadTexture(normalImage);
            this.scale = scale;
        }

        public override void Tick(float deltaTime)
        {
            Vector2 mousePosition = InputManager.mousePosition;
            bool mousePressed = InputManager.GetMouseButtonDown(MouseButton.Left);
            bool mouseDown = InputManager.GetMouseButton(MouseButton.Left);
            bool mouseUp = InputManager.GetMouseButtonUp(MouseButton.Left);
            if (mousePressed)
            {
                if (CheckBounds(mousePosition) && (active))
                {
                    buttonDown.Invoke(name);
                }
            }
            if (mouseUp)
            {
                if (CheckBounds(mousePosition) && (active))
                {
                    buttonUp.Invoke(name);
                }
            }
            if (mouseDown)
            {
                if (CheckBounds(mousePosition) && (active))
                {
                    buttonHold.Invoke(deltaTime, name);
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

            base.Tick(deltaTime);
        }

        public override void Render()
        {
            renderObject.scale = new Vector3(scale.X, scale.Y, 1f);
            renderObject.SetRotation(InfoManager.engineCamera.GetViewMatrix().ExtractRotation().Inverted());
            renderObject.position = addedPosition;
            renderObject.Render();
        }
    }
}