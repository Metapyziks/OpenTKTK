using System;

using OpenTK;

namespace ComputerGraphicsCoursework.Scene
{
    /// <summary>
    /// Class containing the camera's current position and rotation, along
    /// with the perspective and view matrices used when rendering the scene.
    /// </summary>
    public class Camera
    {
        #region Private Fields
        private bool _perspectiveChanged;
        private bool _viewChanged;

        private Matrix4 _perspectiveMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _combinedMatrix;
        private Vector3 _position;
        private Vector2 _rotation;
        #endregion

        /// <summary>
        /// Current width in pixels of the viewport being drawn to.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Current height in pixels of the viewport being drawn to.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Perspective matrix that encodes the transformation from
        /// eye-space to screen-space.
        /// </summary>
        public Matrix4 PerspectiveMatrix
        {
            get
            {
                if (_perspectiveChanged) UpdatePerspectiveMatrix();

                return _perspectiveMatrix;
            }
        }

        /// <summary>
        /// View matrix that encodes the transformation from world-space
        /// to eye-space.
        /// </summary>
        public Matrix4 Viewmatrix
        {
            get
            {
                if (_viewChanged) UpdateViewMatrix();

                return _viewMatrix;
            }
        }

        /// <summary>
        /// Combined view and perspective matrix that encodes the
        /// transformation from world-space to screen-space.
        /// </summary>
        public Matrix4 CombinedMatrix
        {
            get
            {
                if (_perspectiveChanged) UpdatePerspectiveMatrix();
                if (_viewChanged) UpdateViewMatrix();

                return _combinedMatrix;
            }
        }

        /// <summary>
        /// Position of the camera in the world.
        /// </summary>
        public Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// Rotation of the camera, stored as the rotation on the
        /// X and Y axis (pitch and yaw).
        /// </summary>
        public Vector2 Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// The pitch of the camera (rotation on the X axis).
        /// </summary>
        public float Pitch
        {
            get { return _rotation.X; }
            set
            {
                _rotation.X = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// The yaw of the camera (rotation on the Y axis).
        /// </summary>
        public float Yaw
        {
            get { return _rotation.Y; }
            set
            {
                _rotation.Y = value;
                _viewChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the eye normal in world-space.
        /// </summary>
        public Vector3 ViewVector
        {
            get
            {
                float cosYaw = (float) Math.Cos(Yaw);
                float sinYaw = (float) Math.Sin(Yaw);
                float cosPitch = (float) Math.Cos(Pitch);
                float sinPitch = (float) Math.Sin(Pitch);

                // Found through the elegant process of trial and error
                return new Vector3(sinYaw * cosPitch, -sinPitch, -cosYaw * cosPitch);
            }
            set
            {
                // Using a given eye normal, find the pitch and yaw
                value.Normalize();
                Pitch = (float) Math.Asin(-value.Y);
                Yaw = (float) Math.Atan2(value.X, -value.Z);
            }
        }

        /// <summary>
        /// Constructor to create a new Camera instance.
        /// </summary>
        /// <param name="width">Width of the viewport in pixels</param>
        /// <param name="height">Height of the viewport in pixels</param>
        public Camera(int width, int height)
        {
            Width = width;
            Height = height;

            Position = new Vector3();
            Rotation = new Vector2();

            _perspectiveChanged = true;
            _viewChanged = true;
        }

        /// <summary>
        /// Update the dimensions of the viewport.
        /// </summary>
        /// <param name="width">Width of the viewport in pixels</param>
        /// <param name="height">Height of the viewport in pixels</param>
        public void SetScreenSize(int width, int height)
        {
            Width = width;
            Height = height;

            _perspectiveChanged = true;
        }

        /// <summary>
        /// Update the perspective matrix to reflect a change in viewport dimensions.
        /// </summary>
        private void UpdatePerspectiveMatrix()
        {
            _perspectiveChanged = false;

            // Set up a perspective matrix with a 60 degree FOV, the aspect ratio
            // of the current viewport dimensions, some arbitrary depth clip planes
            _perspectiveMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3,
                (float) Width / Height, 1f / 64f, 256f);

            UpdateCombinedMatrix();
        }

        /// <summary>
        /// Update the view matrix to reflect a change in camera position or rotation.
        /// </summary>
        private void UpdateViewMatrix()
        {
            _viewChanged = false;

            Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);  // yaw rotation
            Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);  // pitch rotation
            Matrix4 trns = Matrix4.CreateTranslation(-_position); // position offset

            // Combine the matrices to find the view transformation
            _viewMatrix = Matrix4.Mult(trns, Matrix4.Mult(yRot, xRot));

            UpdateCombinedMatrix();
        }

        /// <summary>
        /// Update the combined view and perspective matrix when either of them changes.
        /// </summary>
        private void UpdateCombinedMatrix()
        {
            _combinedMatrix = Matrix4.Mult(_viewMatrix, _perspectiveMatrix);
        }
    }
}
