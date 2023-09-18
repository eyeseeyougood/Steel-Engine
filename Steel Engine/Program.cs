using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Steel_Engine
{
    public class Program
    {
        private static int Main(string[] args)
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Steel",
                Flags = ContextFlags.ForwardCompatible
            };

            // create window
            SteelWindow window = new SteelWindow(GameWindowSettings.Default, nativeWindowSettings);
            window.Run();
            return 0;
        }
    }
}