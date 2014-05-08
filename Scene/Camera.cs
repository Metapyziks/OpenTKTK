/**
 * Copyright (c) 2013 James King [metapyziks@gmail.com]
 *
 * This file is part of OpenTKTK.
 * 
 * OpenTKTK is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * OpenTKTK is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with OpenTKTK. If not, see <http://www.gnu.org/licenses/>.
 */

using System;

using OpenTK;

namespace OpenTKTK.Scene
{
    /// <summary>
    /// Class containing the camera's current position and rotation, along
    /// with the perspective and view matrices used when rendering the scene.
    /// </summary>
    public class Camera
    {
        private float _fov;
        private float _zNear;
        private float _zFar;

        /// <summary>
        /// Enumeration of positional components.
        /// </summary>
        public enum PositionComponent
        {
            X = 1,
            Y = 2,
            Z = 4,
            All = X | Y | Z
        }

        /// <summary>
        /// Enumeration of rotational components.
        /// </summary>
        public enum RotationComponent
        {
            Pitch = 1,
            Yaw = 2,
            All = Pitch | Yaw
        }

        #region Private Fields
        private bool _projChanged;
        private bool _viewChanged;

        private Matrix4 _projMatrix;
        private Matrix4 _viewMatrix;
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

        public float FieldOfView
        {
            get { return _fov; }
            set
            {
                _fov = value;
                _projChanged = true;
            }
        }

        public float ZNear
        {
            get { return _zNear; }
            set
            {
                _zNear = value;
                _projChanged = true;
            }
        }

        public float ZFar
        {
            get { return _zFar; }
            set
            {
                _zFar = value;
                _projChanged = true;
            }
        }

        /// <summary>
        /// Perspective matrix that encodes the transformation from
        /// eye-space to screen-space.
        /// </summary>
        public Matrix4 ProjectionMatrix
        {
            get
            {
                if (_projChanged) UpdateProjectionMatrix();

                return _projMatrix;
            }
        }

        /// <summary>
        /// View matrix that encodes the transformation from world-space
        /// to eye-space.
        /// </summary>
        public Matrix4 ViewMatrix
        {
            get
            {
                if (_viewChanged) UpdateViewMatrix();

                return _viewMatrix;
            }
        }
        
        /// <summary>
        /// Position of the camera in the world.
        /// </summary>
        public virtual Vector3 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPositionChanged(PositionComponent.All);
            }
        }

        /// <summary>
        /// X component of the camera's position in the world.
        /// </summary>
        public virtual float X
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                OnPositionChanged(PositionComponent.X);
            }
        }

        /// <summary>
        /// Y component of the camera's position in the world.
        /// </summary>
        public virtual float Y
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                OnPositionChanged(PositionComponent.Y);
            }
        }

        /// <summary>
        /// Z component of the camera's position in the world.
        /// </summary>
        public virtual float Z
        {
            get { return _position.Z; }
            set
            {
                _position.Z = value;
                OnPositionChanged(PositionComponent.Z);
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
                OnRotationChanged(RotationComponent.All);
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
                OnRotationChanged(RotationComponent.Pitch);
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
                OnRotationChanged(RotationComponent.Yaw);
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
        public Camera(int width, int height,
            float fov = MathHelper.PiOver3,
            float zNear = 1f / 64f,
            float zFar = 256f)
        {
            Width = width;
            Height = height;

            _zNear = zNear;
            _zFar = zFar;
            _fov = fov;

            _position = new Vector3();
            _rotation = new Vector2();

            InvalidateProjectionMatrix();
            InvalidateViewMatrix();
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

            InvalidateProjectionMatrix();
        }

        /// <summary>
        /// Mark the perspective matrix as requiring an update.
        /// </summary>
        public void InvalidateProjectionMatrix()
        {
            OnInvalidateProjectionMatrix();
            _projChanged = true;
        }

        protected virtual void OnInvalidateProjectionMatrix() { }

        /// <summary>
        /// Update the perspective matrix to reflect a change in viewport dimensions.
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            _projChanged = false;

            OnUpdateProjectionMatrix(ref _projMatrix);
        }

        /// <summary>
        /// Method called when a perspective matrix update should be performed, which
        /// outputs the new perspective matrix.
        /// </summary>
        /// <param name="matrix">The new up-to-date matrix</param>
        protected virtual void OnUpdateProjectionMatrix(ref Matrix4 matrix)
        {
            // Set up a perspective matrix with a 60 degree FOV, the aspect ratio
            // of the current viewport dimensions, some arbitrary depth clip planes
            matrix = Matrix4.CreatePerspectiveFieldOfView(
                FieldOfView, (float) Width / Height, ZNear, ZFar);
        }

        /// <summary>
        /// Mark the view matrix as requiring an update.
        /// </summary>
        public void InvalidateViewMatrix()
        {
            OnInvalidateViewMatrix();
            _viewChanged = true;
        }

        protected virtual void OnInvalidateViewMatrix() { }

        /// <summary>
        /// Update the view matrix to reflect a change in camera position or rotation.
        /// </summary>
        private void UpdateViewMatrix()
        {
            _viewChanged = false;

            OnUpdateViewMatrix(ref _viewMatrix);
        }

        /// <summary>
        /// Method called when a view matrix update should be performed, which
        /// outputs the new view matrix.
        /// </summary>
        /// <param name="matrix">The new up-to-date matrix</param>
        protected virtual void OnUpdateViewMatrix(ref Matrix4 matrix)
        {
            Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);  // yaw rotation
            Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);  // pitch rotation
            Matrix4 trns = Matrix4.CreateTranslation(-_position); // position offset

            // Combine the matrices to find the view transformation
            matrix = Matrix4.Mult(trns, Matrix4.Mult(yRot, xRot));
        }

        private void OnPositionChanged(PositionComponent component)
        {
            OnPositionChanged(component, ref _position);
        }

        /// <summary>
        /// Method called when the camera's position has been modified.
        /// </summary>
        /// <param name="component">The component(s) of the camera's position
        /// that were modified</param>
        protected virtual void OnPositionChanged(PositionComponent component, ref Vector3 position)
        {
            InvalidateViewMatrix();
        }

        private void OnRotationChanged(RotationComponent component)
        {
            OnRotationChanged(component, ref _rotation);
        }

        /// <summary>
        /// Method called when the camera's rotation has been modified.
        /// </summary>
        /// <param name="component">The component(s) of the camera's rotation
        /// that were modified</param>
        protected virtual void OnRotationChanged(RotationComponent component, ref Vector2 rotation)
        {
            InvalidateViewMatrix();
        }
    }
}
