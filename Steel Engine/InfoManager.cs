using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using Steel_Engine.Common;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class InfoManager
    {
        public static string currentDevPath = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\";
        public static string devDataPath = @"C:\Users\Mati\source\repos\Steel Engine\Steel Engine\Resources\";
        public static string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string dataPath = AppDomain.CurrentDomain.BaseDirectory + @"\Resources";

        public static string usingDirectory = "";
        public static string usingDataPath = "";

        public static float gravityStrength = 1f;

        public static GameObject testObject; //TESTCODE

        public static Camera engineCamera;

        public static Vector2 windowSize;
        public static Vector2i windowPosition;

        public static bool isBuild;

        public static string[] executingArgs;

        public static bool lockWindowSize;

        private static Vector2 windowLock;
        private static WindowState windowStateLock;

        public delegate void OnSetWindowSize(int width, int height);
        public static event OnSetWindowSize setWindowSize;

        public delegate void OnSetWindowState(WindowState state);
        public static event OnSetWindowState setWindowState;

        public delegate void OnSetWindowTitle(string title);
        public static event OnSetWindowTitle setWindowTitle;

        public delegate void OnSetWindowPosition(Vector2i position);
        public static event OnSetWindowPosition setWindowPosition;

        public static void Tick(float deltaTime)
        {
            usingDirectory = currentDevPath;
            usingDataPath = devDataPath;
            if (isBuild)
            {
                usingDirectory = currentDir;
                usingDataPath = dataPath;
            }
            if (lockWindowSize)
            {
                SetWindowState(windowStateLock);
                SetWindowSize(windowLock);
            }
        }

        public static void SetWindowSize(Vector2 windowSize)
        {
            windowLock = windowSize;
            setWindowSize.Invoke((int)windowSize.X, (int)windowSize.Y);
        }

        public static void SetWindowState(WindowState state)
        {
            windowStateLock = state;
            setWindowState.Invoke(state);
        }

        public static void SetWindowTitle(string title)
        {
            setWindowTitle.Invoke(title);
        }

        public static void SetWindowPosition(Vector2i position)
        {
            setWindowPosition.Invoke(position);
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;

            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                IntPtr ptr = bmpdata.Scan0;
                byte[] imageData = new byte[numbytes];

                byte[] temp = new byte[numbytes];
                Marshal.Copy(ptr, temp, 0, numbytes);

                // Iterate over each row and copy the data in reverse order
                for (int y = 0; y < bitmap.Height; y++)
                {
                    int sourceIndex = y * bmpdata.Stride;
                    int destinationIndex = (bitmap.Height - 1 - y) * bmpdata.Stride;

                    // Copy the entire row
                    Buffer.BlockCopy(temp, sourceIndex, imageData, destinationIndex, bmpdata.Stride);
                }

                imageData = ConvertRGBAtoBGRA(imageData);

                return imageData;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }

        public static byte[] ConvertRGBAtoBGRA(byte[] argbData)
        {
            // Convert ARGB to RGBA format
            byte[] rgbaData = new byte[argbData.Length];
            for (int i = 0; i < argbData.Length - 3; i += 4)
            {
                rgbaData[i] = argbData[i + 2]; // R
                rgbaData[i + 1] = argbData[i + 1]; // G
                rgbaData[i + 2] = argbData[i]; // B
                rgbaData[i + 3] = argbData[i + 3]; // A
            }
            return rgbaData;
        }

        /*
 *             List<byte> result = new List<byte>();

    int y = 0;
    while (y < bitmap.Height - 1)
    {
        int x = 0;
        while (x < bitmap.Width - 1)
        {
            result.Add(bitmap.GetPixel(x, y).R);
            result.Add(bitmap.GetPixel(x, y).G);
            result.Add(bitmap.GetPixel(x, y).B);
            result.Add(bitmap.GetPixel(x, y).A);
            x++;
        }
        y++;
    }

    return result.ToArray();
 * */
    }
}