using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steel_Engine
{
    public static class InputManager
    {
        private static KeyboardState keyboardState;
        private static MouseState mouseState;
        private static CursorState cursorState;
        public static Vector2 mousePosition;

        public static void Tick(KeyboardState keyState, MouseState mState, CursorState cState)
        {
            keyboardState = keyState;
            mouseState = mState;
            cursorState = cState;
            mousePosition = mouseState.Position;
        }

        public static CursorState GetCursorState()
        {
            return cursorState;
        }

        public static bool GetKeyDown(Keys key)
        {
            return keyboardState.IsKeyPressed(key);
        }

        public static bool GetKey(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }

        public static bool GetKeyUp(Keys key)
        {
            return keyboardState.IsKeyReleased(key);
        }

        public static bool GetMouseButtonDown(MouseButton button)
        {
            return mouseState.IsButtonPressed(button);
        }

        public static bool GetMouseButton(MouseButton button)
        {
            return mouseState.IsButtonDown(button);
        }

        public static bool GetMouseButtonUp(MouseButton button)
        {
            return mouseState.IsButtonReleased(button);
        }
    }
}
