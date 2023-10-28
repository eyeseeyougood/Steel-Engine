using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace Steel_Engine.Common
{
    public enum ProjectionType
    {
        Orthographic = 0,
        Perspective = 1
    }

    public class Camera
    {
        // Those vectors are directions pointing outwards from the camera to define how it rotated.
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        // Rotation around the X axis (radians)
        private float _pitch = 0;

        // Rotation around the Y axis (radians)
        private float _yaw = -MathHelper.PiOver2; // Without this, you would be started rotated 90 degrees right.

        // The field of view of the camera (radians)
        private float _fov = MathHelper.PiOver2;

        private bool firstMove;
        private Vector2 lastPos;

        public float cameraSpeed = 1.5f;
        public float sensitivity = 0.2f;

        public string name;
        public int id;

        public void SetMain()
        {
            if (InfoManager.isBuild)
                InfoManager.engineCamera = this;
        }

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        // The position of the camera
        public Vector3 Position { get; set; }

        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { get; set; }

        public Vector3 Front => _front;

        public Vector3 Up => _up;

        public Vector3 Right => _right;

        public ProjectionType projectionType = ProjectionType.Perspective;

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                // We clamp the pitch value between -89 and 89 to prevent the camera from going upside down, and a bunch
                // of weird "bugs" when you are using euler angles for rotation.
                // If you want to read more about this you can try researching a topic called gimbal lock
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public void Tick(double deltaTime)
        {
            if (InputManager.GetCursorState() == CursorState.Normal)
                return;

            if (InfoManager.isBuild)
                return;

            if (InfoManager.engineCamera != this)
                return;

            Vector2 mousePosition = InputManager.mousePosition;

            if (firstMove)
            {
                lastPos = new Vector2(mousePosition.X, mousePosition.Y);
                firstMove = false;
            }
            else
            {
                var deltaX = mousePosition.X - lastPos.X;
                var deltaY = mousePosition.Y - lastPos.Y;
                lastPos = new Vector2(mousePosition.X, mousePosition.Y);

                Yaw += deltaX * sensitivity;
                Pitch -= deltaY * sensitivity;
            }

            if (InputManager.GetKey(Keys.W))
            {
                Position += Front * cameraSpeed * (float)deltaTime;
            }

            if (InputManager.GetKey(Keys.S))
            {
                Position -= Front * cameraSpeed * (float)deltaTime;
            }

            if (InputManager.GetKey(Keys.A))
            {
                Position -= Right * cameraSpeed * (float)deltaTime;
            }

            if (InputManager.GetKey(Keys.D))
            {
                Position += Right * cameraSpeed * (float)deltaTime;
            }

            if (InputManager.GetKey(Keys.E))
            {
                Position += Up * cameraSpeed * (float)deltaTime;
            }

            if (InputManager.GetKey(Keys.Q))
            {
                Position -= Up * cameraSpeed * (float)deltaTime;
            }
        }

        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        // The field of view (FOV) is the vertical angle of the camera view.
        // This has been discussed more in depth in a previous tutorial,
        // but in this tutorial, you have also learned how we can use this to simulate a zoom feature.
        // We convert from degrees to radians as soon as the property is set to improve performance.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 179f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            if (projectionType == ProjectionType.Orthographic)
            {
                return Matrix4.CreateOrthographic(Fov, Fov*(InfoManager.windowSize.Y / InfoManager.windowSize.X), 0.01f, 100f);
            }
            return Matrix4.CreatePerspectiveFieldOfView(_fov, InfoManager.windowSize.X / InfoManager.windowSize.Y, 0.01f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private void UpdateVectors()
        {
            // First, the front matrix is calculated using some basic trigonometry.
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);

            // We need to make sure the vectors are all normalized, as otherwise we would get some funky results.
            _front = Vector3.Normalize(_front);

            // Calculate both the right and the up vector using cross product.
            // Note that we are calculating the right from the global up; this behaviour might
            // not be what you need for all cameras so keep this in mind if you do not want a FPS camera.
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}