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
        public enum PositionComponent
        {
            X = 1,
            Y = 2,
            Z = 4,
            All = X | Y | Z
        }

        public enum RotationComponent
        {
            Pitch = 1,
            Yaw = 2,
            All = Pitch | Yaw
        }

        #region Private Fields
        private bool _perspectiveChanged;
        private bool _viewChanged;
        private bool _combinedChanged;

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
                if (_combinedChanged) UpdateCombinedMatrix();

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
                OnPositionChanged(PositionComponent.All);
            }
        }

        /// <summary>
        /// X component of the camera's position in the world.
        /// </summary>
        public float X
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
        public float Y
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
        public float Z
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
        public Camera(int width, int height)
        {
            Width = width;
            Height = height;

            Position = new Vector3();
            Rotation = new Vector2();

            InvalidatePerspectiveMatrix();
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

            InvalidatePerspectiveMatrix();
        }

        /// <summary>
        /// Mark the perspective matrix as requiring an update.
        /// </summary>
        public void InvalidatePerspectiveMatrix()
        {
            _perspectiveChanged = true;
            _combinedChanged = true;
        }

        /// <summary>
        /// Update the perspective matrix to reflect a change in viewport dimensions.
        /// </summary>
        private void UpdatePerspectiveMatrix()
        {
            _perspectiveChanged = false;

            OnUpdatePerspectiveMatrix(ref _perspectiveMatrix);
        }

        /// <summary>
        /// Method called when a perspective matrix update should be performed, which
        /// outputs the new perspective matrix.
        /// </summary>
        /// <param name="matrix">The new up-to-date matrix</param>
        protected virtual void OnUpdatePerspectiveMatrix(ref Matrix4 matrix)
        {
            // Set up a perspective matrix with a 60 degree FOV, the aspect ratio
            // of the current viewport dimensions, some arbitrary depth clip planes
            matrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3,
                (float) Width / Height, 1f / 64f, 256f);
        }

        /// <summary>
        /// Mark the view matrix as requiring an update.
        /// </summary>
        public void InvalidateViewMatrix()
        {
            _viewChanged = true;
            _combinedChanged = true;
        }

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
        protected void OnUpdateViewMatrix(ref Matrix4 matrix)
        {
            Matrix4 yRot = Matrix4.CreateRotationY(_rotation.Y);  // yaw rotation
            Matrix4 xRot = Matrix4.CreateRotationX(_rotation.X);  // pitch rotation
            Matrix4 trns = Matrix4.CreateTranslation(-_position); // position offset

            // Combine the matrices to find the view transformation
            matrix = Matrix4.Mult(trns, Matrix4.Mult(yRot, xRot));
        }

        /// <summary>
        /// Update the combined view and perspective matrix when either of them changes.
        /// </summary>
        private void UpdateCombinedMatrix()
        {
            _combinedChanged = false;

            _combinedMatrix = Matrix4.Mult(_viewMatrix, _perspectiveMatrix);
        }

        /// <summary>
        /// Method called when the camera's position has been modified.
        /// </summary>
        /// <param name="component">The component(s) of the camera's position
        /// that were modified</param>
        protected virtual void OnPositionChanged(PositionComponent component)
        {
            InvalidateViewMatrix();
        }

        /// <summary>
        /// Method called when the camera's rotation has been modified.
        /// </summary>
        /// <param name="component">The component(s) of the camera's rotation
        /// that were modified</param>
        protected virtual void OnRotationChanged(RotationComponent component)
        {
            InvalidateViewMatrix();
        }
    }
}
