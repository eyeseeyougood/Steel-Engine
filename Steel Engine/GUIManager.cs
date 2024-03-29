﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steel_Engine.Common;
using System.ComponentModel;

namespace Steel_Engine.GUI
{
    public static class GUIManager
    {
        public static List<GUIElement> guiElements = new List<GUIElement>();

        public static GUIElement selectedhierarchyObject = null;

        public delegate void GUITick(float deltaTime);
        public static event GUITick guiTick = new GUITick(GT);

        private static void GT(float d) { }

        public static void LoadEngineGUI()
        {
            // TopBar
            GUIImage topBarPanel = new GUIImage(new Vector3(0, -12.5f, 0), new Vector2(0f, -1f), new Vector2(1.88f, 0.12f), new Vector3(50, 50, 50)/255.0f);
            topBarPanel.name = "topbarBG";
            topBarPanel.renderOrder = -2;

            GUIText simulatingText = new GUIText(new Vector3(0, -3.5f, 0), new Vector2(0f, -1f), 0.07f, "Not Simulating", @"C:\Windows\Fonts\Arial.ttf", 300f, new Vector4(0, 0, 0, 50), new Vector4(230, 230, 230, 255));
            simulatingText.PreloadText("Simulating");
            simulatingText.name = "topbarSimText";
            simulatingText.renderOrder = -1;

            string arrowPath = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\Arrow1.png";
            string arrow2Path = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\Arrow2.png";
            string plusB1Path = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\PlusButton1.png";
            string plusB2Path = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\PlusButton2.png";
            string PanelUnpressed = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\PanelUnpressed.png";
            string PanelPressed = InfoManager.usingDirectory + @$"EngineResources\EngineTextures\PanelPressed.png";
            GUIButton xpButton = new GUIButton(new Vector3(0, -12.5f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            xpButton.SetPressedImage(arrow2Path);
            xpButton.buttonHold += EngineGUIEventManager.XPButtonHold;
            xpButton.SetZRotation(0);
            xpButton.renderOrder = -1;
            
            GUIButton xmButton = new GUIButton(new Vector3(-30f, -12.5f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            xmButton.SetPressedImage(arrow2Path);
            xmButton.buttonHold += EngineGUIEventManager.XMButtonHold;
            xmButton.SetZRotation(180);
            xmButton.renderOrder = -1;

            GUIButton ypButton = new GUIButton(new Vector3(-15f, -5f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            ypButton.SetPressedImage(arrow2Path);
            ypButton.buttonHold += EngineGUIEventManager.YPButtonHold;
            ypButton.SetZRotation(-90);
            ypButton.renderOrder = -1;

            GUIButton ymButton = new GUIButton(new Vector3(-15f, -20f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            ymButton.SetPressedImage(arrow2Path);
            ymButton.buttonHold += EngineGUIEventManager.YMButtonHold;
            ymButton.SetZRotation(90);
            ymButton.renderOrder = -1;

            GUIButton zpButton = new GUIButton(new Vector3(15f, -5f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            zpButton.SetPressedImage(arrow2Path);
            zpButton.buttonHold += EngineGUIEventManager.ZPButtonHold;
            zpButton.SetZRotation(-90);
            zpButton.renderOrder = -1;

            GUIButton zmButton = new GUIButton(new Vector3(15f, -20f, 0), new Vector2(-0.5f, -1f), new Vector2(0.05f, 0.05f), arrowPath);
            zmButton.SetPressedImage(arrow2Path);
            zmButton.buttonHold += EngineGUIEventManager.ZMButtonHold;
            zmButton.SetZRotation(90);
            zmButton.renderOrder = -1;

            GUIButton createEmpty = new GUIButton(new Vector3(20f, -22f, 0), new Vector2(-1f, -1f), new Vector2(0.05f, 0.05f), plusB1Path);
            createEmpty.SetPressedImage(plusB2Path);
            createEmpty.buttonDown += EngineGUIEventManager.CreateEmpty;
            createEmpty.renderOrder = -1;

            // Rebuild
            GUIButton rebuildButton = new GUIButton(new Vector3(-2f, -15f, 0), new Vector2(-1f, -1f), new Vector2(0.09f, 0.09f), PanelUnpressed);
            rebuildButton.SetPressedImage(PanelPressed);
            rebuildButton.buttonHold += EngineGUIEventManager.UnlockRebuild;
            rebuildButton.renderOrder = -1;
            //rebuildButton.visible = false;

            GUIText rebuildText1 = new GUIText(new Vector3(-2f, -11f, 0), new Vector2(-1f, -1f), 0.04f, "Unlock", @"C:\Windows\Fonts\Arial.ttf", 300f, new Vector4(0, 0, 0, 0));
            rebuildText1.name = "RebuildText1";
            rebuildText1.renderOrder = 0;

            GUIText rebuildText2 = new GUIText(new Vector3(-2f, -19f, 0), new Vector2(-1f, -1f), 0.04f, "Rebuild", @"C:\Windows\Fonts\Arial.ttf", 300f, new Vector4(0, 0, 0, 0));
            rebuildText2.name = "RebuildText2";
            rebuildText2.renderOrder = 0;

            // hierarchy
            GUIScrollView hierarchy = new GUIScrollView(new Vector3(39, -158f, 0), new Vector2(-1f, -1f), new Vector2(0.4f, 0.9f));
            hierarchy.SetColour(new Vector3(45, 45, 45) / 255.0f);
            hierarchy.name = "hierarchyBG";
            hierarchy.renderOrder = -2;
            hierarchy.padding = 0.01f;
            hierarchy.spacing = 0.01f;
            hierarchy.scrollStrength = 1.08f;

            // inspector
            GUIScrollView inspectorView = new GUIScrollView(new Vector3(-39, -158f, 0), new Vector2(1f, -1f), new Vector2(0.4f, 0.9f));
            inspectorView.name = "inspectorView";
            inspectorView.padding = 0.01f;
            inspectorView.spacing = 0.01f;
            inspectorView.scrollStrength = 1.025f;
            inspectorView.renderOrder = -2;
            inspectorView.SetColour(new Vector3(45, 45, 45) / 255.0f);
            inspectorView.visible = true;
            inspectorView.active = true;

            GUIButton addComponentButton = new GUIButton(new Vector3(-38f, -20f, 0), new Vector2(1f, -1f), new Vector2(0.285f, 0.05f));
            addComponentButton.visible = false;
            addComponentButton.buttonDown += EngineGUIEventManager.ToggleComponentViewEvent;
            addComponentButton.renderOrder = -1;

            GUIText addComponentText = new GUIText(Vector3.Zero, Vector2.Zero, 0.07f, "Add Component", @"C:\Windows\Fonts\Arial.ttf", 300f, new Vector4(0, 0, 0, 50), new Vector4(230, 230, 230, 255));
            addComponentText.name = "addComponentText";
            addComponentText.parentGUI = addComponentButton;

            GUIScrollView componentsView = new GUIScrollView(new Vector3(-39, 0, 0), new Vector2(1, 0), new Vector2(0.4f, 0.7f));
            componentsView.name = "componentsView";
            componentsView.padding = 0.01f;
            componentsView.spacing = 0.01f;
            componentsView.scrollStrength = 1.025f;
            componentsView.renderOrder = 1;
            componentsView.SetColour(new Vector3(0.3f, 0.3f, 0.3f));
            componentsView.visible = false;
            componentsView.active = false;

            AddGUIElement(simulatingText);
            AddGUIElement(xpButton);
            AddGUIElement(xmButton);
            AddGUIElement(ypButton);
            AddGUIElement(ymButton);
            AddGUIElement(zpButton);
            AddGUIElement(zmButton);
            AddGUIElement(topBarPanel);
            AddGUIElement(rebuildButton);
            AddGUIElement(rebuildText1);
            AddGUIElement(rebuildText2);
            AddGUIElement(createEmpty);
            AddGUIElement(inspectorView);
            AddGUIElement(addComponentButton);
            AddGUIElement(addComponentText);
            AddGUIElement(componentsView);
            AddGUIElement(hierarchy);

            RefreshComponentsMenu();

            Refreshhierarchy();
        }

        public static void RefreshInspectorMenu()
        {
            // remove old entries
            GUIScrollView inspectorView = ((GUIScrollView)GetElementByName("inspectorView"));
            foreach (GUIElement element in inspectorView.contents)
            {
                GameObject go = SceneManager.gameObjects[int.Parse(element.name.Split(' ')[0])];

                foreach (Component component in go.components)
                {
                    string elementName = element.name.Split(' ')[0] + " " + component.GetType().Name + "ComponentText";
                    guiElements.Remove(GetElementByName(elementName));
                }
                guiTick -= element.Tick;
            }
            inspectorView.contents.Clear();

            // add new entries
            if (selectedhierarchyObject != null)
            {
                // there is a selected object
                GameObject go = SceneManager.gameObjects[int.Parse(selectedhierarchyObject.name.Split(' ')[0])];
                foreach (Component component in go.components)
                {
                    GUIImage componentPanel = new GUIImage(new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0.39f, 0.03f), new Vector3(35, 35, 35)/255.0f);
                    componentPanel.name = go.id + " " + $"{component.GetType().Name}ComponentImage";
                    componentPanel.renderOrder = -1;
                    componentPanel.onRender += component.OnDrawInspectorPanel;
                    componentPanel.onRendered += component.OnInspectorPanelDrawn;
                    GUIText componentText = new GUIText(new Vector3(0, -4f, 0), new Vector2(0, -1f), 0.3f, component.GetType().Name, @"C:\Windows\Fonts\Arial.ttf", 45, new Vector4(0, 0, 0, 0), new Vector4(225, 225, 225, 255));
                    componentText.name = go.id + " " + component.GetType().Name + "ComponentText";
                    componentText.parentGUI = componentPanel;
                    componentText.renderOrder = 0;
                    AddGUIElement(componentText);
                    inspectorView.contents.Add(componentPanel);
                }
            }
        }

        public static void RefreshComponentsMenu()
        {
            // doesn't delete old entries
            GUIScrollView view = (GUIScrollView)GetElementByName("componentsView");
            foreach (Type t in typeof(Component).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Component))))
            {
                string _namespace = "";
                if (t.Namespace != null)
                {
                    if (t.Namespace.Length > 12)
                    {
                        _namespace = t.Namespace;
                    }
                }
                GUIButton componentButton = new GUIButton(new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0.39f, 0.03f), InfoManager.usingDirectory + @$"EngineResources\EngineTextures\TestUp.png");
                componentButton.name = $"{_namespace}.{t.Name} ComponentButton";
                componentButton.renderOrder = 1;
                componentButton.buttonDown += EngineGUIEventManager.AddComponentEvent;
                componentButton.SetPressedImage(InfoManager.usingDirectory + @$"EngineResources\EngineTextures\TestDown.png");
                GUIText componentText = new GUIText(new Vector3(0, 0, 0), new Vector2(0, 0), 0.3f, t.Name, @"C:\Windows\Fonts\Arial.ttf", 45, new Vector4(0,0,0,0), new Vector4(225, 225, 225, 255));
                componentText.name = t.Name+" ComponentText";
                componentText.parentGUI = componentButton;
                componentText.renderOrder = 2;
                AddGUIElement(componentText);
                view.contents.Add(componentButton);
            }
        }

        public static void Refreshhierarchy()
        {
            GUIScrollView hierarchy = (GUIScrollView)GetElementByName("hierarchyBG");
            hierarchy.contents.Clear();
            foreach (GameObject gameObject in SceneManager.gameObjects)
            {
                GUIButton hierarchyButtonObject = new GUIButton(new Vector3(0, 0, 0), new Vector2(0, 0), new Vector2(0.38f, 0.03f));
                hierarchyButtonObject.visible = false;
                hierarchyButtonObject.active = true;
                hierarchyButtonObject.renderOrder = -1;
                hierarchyButtonObject.name = gameObject.id.ToString() + " button object";
                hierarchyButtonObject.buttonDown += EngineGUIEventManager.SelecthierarchyObject;
                GUIImage hierarchyImageObject = new GUIImage(Vector3.Zero, Vector2.Zero, new Vector2(0.38f, 0.03f), new Vector4(0, 0, 0, 100));
                hierarchyImageObject.parentGUI = hierarchyButtonObject;
                hierarchyImageObject.name = gameObject.id.ToString() + " image object";
                hierarchyImageObject.localRenderOrder = 0;
                GUIText hierarchyTextObject = new GUIText(Vector3.Zero, Vector2.Zero, 0.07f, gameObject.name, @"C:\Windows\Fonts\Arial.ttf", 200f, new Vector4(0, 0, 0, 0), new Vector4(200, 200, 200, 255));
                hierarchyTextObject.name = gameObject.id.ToString() + " text object";
                hierarchyTextObject.parentGUI = hierarchyButtonObject;
                hierarchyTextObject.localRenderOrder = 1;
                AddGUIElement(hierarchyTextObject);
                AddGUIElement(hierarchyImageObject);
                hierarchy.contents.Add(hierarchyButtonObject);
            }
        }

        public static void Tick(float deltaTime)
        {
            guiTick.Invoke(deltaTime);
            if (!InfoManager.isBuild)
            {
                if (GetElementByName("componentsView").visible && selectedhierarchyObject == null)
                {
                    GetElementByName("componentsView").visible = false;
                }
            }
        }

        public static void Render()
        {
            // sort list before render according to the render order of each item
            guiElements.Sort(new GUIElementComparer());
            foreach (GUIElement guiElement in guiElements)
            {
                guiElement.Render();
            }
        }

        public static GUIElement GetElementByID(int id)
        {
            GUIElement element = guiElements[id];
            return element;
        }

        public static GUIElement GetElementByName(string name)
        {
            foreach (GUIElement element in guiElements)
            {
                if (element.name == name)
                {
                    return element;
                }
                else if (element.GetType() == typeof(GUIScrollView))
                {
                    foreach (GUIElement _elem in ((GUIScrollView)element).contents)
                    {
                        if (_elem.name == name)
                        {
                            return _elem;
                        }
                    }
                }
            }
            return null;
        }

        public static void AddGUIElement(GUIElement element)
        {
            guiElements.Add(element);
        }

        public static void Cleanup()
        {
            // nothing to clean up atm
        }

        public static Bitmap Write_Text(string text, string fontName, float size)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(result.Width)+1, (int)MathF.Round(result.Height)+1);
            Rectangle rect = new Rectangle();
            rect.Width = (int)MathF.Round(result.Width);
            rect.Height = (int)MathF.Round(result.Height);
            rect.X = 0;
            rect.Y = 0;
            Color bgColour = Color.FromArgb(255, Color.White);
            return Write_Text(bmp, text, rect, true, fontName, false, false, bgColour);
        }

        public static Bitmap Write_Text(string text, string fontName, float size, Vector4 bg255)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(result.Width) + 1, (int)MathF.Round(result.Height) + 1);
            Rectangle rect = new Rectangle();
            rect.Width = (int)MathF.Round(result.Width);
            rect.Height = (int)MathF.Round(result.Height);
            rect.X = 0;
            rect.Y = 0;
            Color bgColour = Color.FromArgb((int)bg255.W, (int)bg255.X, (int)bg255.Y, (int)bg255.Z);
            return Write_Text(bmp, text, rect, true, fontName, false, false, bgColour);
        }

        public static Bitmap Write_Text(string text, string fontName, float size, Vector4 bg255, Vector4 font255)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(result.Width) + 1, (int)MathF.Round(result.Height) + 1);
            Rectangle rect = new Rectangle();
            rect.Width = (int)MathF.Round(result.Width);
            rect.Height = (int)MathF.Round(result.Height);
            rect.X = 0;
            rect.Y = 0;
            Color bgColour = Color.FromArgb((int)bg255.W, (int)bg255.X, (int)bg255.Y, (int)bg255.Z);
            Color fontColour = Color.FromArgb((int)font255.W, (int)font255.X, (int)font255.Y, (int)font255.Z);
            return Write_Text(bmp, text, rect, true, fontName, false, false, bgColour, fontColour);
        }

        public static Bitmap Write_Text(string text, string fontName, float size, Rectangle destRect, Rectangle fullRect)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(fullRect.Width) + 1, (int)MathF.Round(fullRect.Height) + 1);
            Color bgColour = Color.FromArgb(255, Color.White);
            return Write_Text(bmp, text, destRect, true, fontName, false, false, bgColour);
        }

        public static Bitmap Write_Text(string text, string fontName, float size, Vector4 bg255, Rectangle destRect, Rectangle fullRect)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(fullRect.Width) + 1, (int)MathF.Round(fullRect.Height) + 1);
            Color bgColour = Color.FromArgb((int)bg255.W, (int)bg255.Y, (int)bg255.Z, (int)bg255.X);
            return Write_Text(bmp, text, destRect, true, fontName, false, false, bgColour);
        }

        public static Bitmap Write_Text(string text, string fontName, float size, Vector4 bg255, Vector4 txt255, Rectangle destRect, Rectangle fullRect)
        {
            Font f = new Font(fontName, size, GraphicsUnit.Pixel);
            SizeF result;
            using (Bitmap b = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(b))
                {
                    result = g.MeasureString(text, f);
                }
            }
            Bitmap bmp = new Bitmap((int)MathF.Round(fullRect.Width) + 1, (int)MathF.Round(fullRect.Height) + 1);
            Color bgColour = Color.FromArgb((int)bg255.W, (int)bg255.Y, (int)bg255.Z, (int)bg255.X);
            Color textColour = Color.FromArgb((int)txt255.W, (int)txt255.Y, (int)txt255.Z, (int)txt255.X);
            return Write_Text(bmp, text, destRect, true, fontName, false, false, bgColour, textColour);
        }

        public static Bitmap Write_Text(Bitmap i, string s, Rectangle r, bool centered, string font, bool bold, bool italic, Color bgColour)
        {
            //Since we want to avoid out of bounds errors make sure the rectangle remains within the bounds of the bitmap
            //and only execute if it does
            if (r.X >= 0 && r.Y >= 0 && (r.X + r.Width < i.Width) && (r.Y + r.Height < i.Height))
            {
                //Step one is to make a graphics object that will draw the text in place
                using (Graphics g = Graphics.FromImage(i))
                {
                    //Set some of the graphics properties so that the text renders nicely
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //Compositing Mode can't be set since string needs source over to be valid
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    //And an additional step to make sure text is proper anti-aliased and takes advantage
                    //of clear type as necessary
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    //this also requires a font object we need to make sure we dispose of properly
                    using (Font f = Generate_Font(s, font, r, bold, italic))
                    {
                        //the using function actually makes sure the font is as large as it can be for the 
                        //purpose of fitting the rectangle we just need to check if its centered
                        using (StringFormat format = new StringFormat())
                        {
                            //the format won't always be doing anything but
                            //just in case we need it
                            //and if the text is centered we need to tell the formatting
                            if (centered)
                            {
                                format.Alignment = StringAlignment.Center;
                                format.Alignment = StringAlignment.Center;
                            }
                            //and draw the text into place
                            g.DrawString(s, f, Brushes.Black, r, format);
                        }
                    }
                }
            }
            int y = 0;
            while (y < i.Height)
            {
                int x = 0;
                while (x < i.Width)
                {
                    if (i.GetPixel(x, y) == Color.FromArgb(0, 0, 0, 0))
                    {
                        i.SetPixel(x, y, bgColour);
                    }
                    x++;
                }
                y++;
            }
            return i;
        }

        public static Bitmap Write_Text(Bitmap i, string s, Rectangle r, bool centered, string font, bool bold, bool italic, Color bgColour, Color fontColour)
        {
            //Since we want to avoid out of bounds errors make sure the rectangle remains within the bounds of the bitmap
            //and only execute if it does
            if (r.X >= 0 && r.Y >= 0 && (r.X + r.Width < i.Width) && (r.Y + r.Height < i.Height))
            {
                //Step one is to make a graphics object that will draw the text in place
                using (Graphics g = Graphics.FromImage(i))
                {
                    //Set some of the graphics properties so that the text renders nicely
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    //Compositing Mode can't be set since string needs source over to be valid
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    //And an additional step to make sure text is proper anti-aliased and takes advantage
                    //of clear type as necessary
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    //this also requires a font object we need to make sure we dispose of properly
                    using (Font f = Generate_Font(s, font, r, bold, italic))
                    {
                        //the using function actually makes sure the font is as large as it can be for the 
                        //purpose of fitting the rectangle we just need to check if its centered
                        using (StringFormat format = new StringFormat())
                        {
                            //the format won't always be doing anything but
                            //just in case we need it
                            //and if the text is centered we need to tell the formatting
                            if (centered)
                            {
                                format.Alignment = StringAlignment.Center;
                                format.Alignment = StringAlignment.Center;
                            }
                            //and draw the text into place
                            g.DrawString(s, f, Brushes.Black, r, format);
                        }
                    }
                }
            }
            int y = 0;
            while (y < i.Height)
            {
                int x = 0;
                while (x < i.Width)
                {
                    if (i.GetPixel(x, y) == Color.FromArgb(0, 0, 0, 0))
                    {
                        i.SetPixel(x, y, bgColour);
                    }
                    else if (i.GetPixel(x, y) == Color.FromArgb(255, 0, 0, 0))
                    {
                        i.SetPixel(x, y, fontColour);
                    }
                    x++;
                }
                y++;
            }
            return i;
        }

        public static Font Generate_Font(string s, string font_family, Rectangle r, bool bold, bool italic)
        {
            //First things first, the font can't be of a size larger than the rectangle in pixels so 
            //we need to find the smaller dimension as that will constrain the max size
            int Max_Size = Math.Min(r.Width, r.Height);
            //Now we loop backwards from this max size until we find a size of font that fits inside the 
            //rectangle given
            for (int size = Max_Size; size > 0; size--)
            {
                //Since a default font is used if the font family specified doesnt exist 
                //checking the family exists isnt necessary
                //However we need to cover if the font is bold or italic
                Font f;
                if (bold)
                {
                    f = new Font(font_family, size, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);
                }
                else if (italic)
                {
                    f = new Font(font_family, size, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                }
                else if (bold && italic)
                {
                    //the pipe is a bitwise or and plays with the enum flags to get both bold and italic 
                    f = new Font(font_family, size, System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                }
                else
                {
                    //otherwise make a simple font
                    f = new Font(font_family, size, GraphicsUnit.Pixel);
                }
                //because graphics are weird we need a bitmap and graphics object to measure the string
                //we also need a sizef to store the measured results
                SizeF result;
                using (Bitmap b = new Bitmap(100, 100))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        result = g.MeasureString(s, f);
                    }
                }
                //if the new string fits the constraints of the rectangle we return it
                if (result.Width <= r.Width && result.Height <= r.Height)
                {
                    return f;
                }
                //if it didnt we dispose of f and try again
                f.Dispose();
            }
            //If something goes horribly wrong and no font size fits just return comic sans in 12 pt font
            //that won't upset anyone and the rectangle it will be drawn to will clip the excess anyway
            return new Font("Comic Sans", 12, GraphicsUnit.Point);
        }
    }
}
