using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine.GUI
{
    public class GUIManager
    {
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

        public static Bitmap Write_Text(string text, string fontName, float size, Color bg)
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
            return Write_Text(bmp, text, rect, true, fontName, false, false, bg);
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
