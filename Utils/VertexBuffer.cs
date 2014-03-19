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
using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKTK.Shaders;

namespace OpenTKTK.Utils
{
    /// <summary>
    /// Class that creates and manages an OpenGL vertex buffer object (VBO). 
    /// </summary>
    public class VertexBuffer : IDisposable
    {
        #region Private Fields
        private BufferUsageHint _usageHint;

        private int _unitSize;
        private int _vboID;
        private int _dataLength;

        private bool _dataSet;
        private ShaderProgram _curShader;
        #endregion

        protected BufferUsageHint UsageHint
        {
            get { return _usageHint; }
        }

        protected ShaderProgram CurrentShader
        {
            get { return _curShader; }
        }

        /// <summary>
        /// Identification number assigned by OpenGL when the VBO is created.
        /// </summary>
        public int VboID
        {
            get
            {
                // If the VBO doesn't exist yet, create it
                if (_vboID == 0) GL.GenBuffers(1, out _vboID);

                return _vboID;
            }
        }

        public virtual bool DataSet { get { return _dataSet; } }

        /// <summary>
        /// Number of floats per vertex in the VBO.
        /// </summary>
        public int Stride { get; private set; }

        /// <summary>
        /// Constructor to create a new VertexBuffer instance.
        /// </summary>
        /// <param name="stride">The number of floats per vertex</param>
        public VertexBuffer(int stride, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            Stride = stride;

            _usageHint = usageHint;

            _vboID = 0;
            _dataSet = false;
        }

        /// <summary>
        /// Populates the VBO with vertex data.
        /// </summary>
        /// <typeparam name="T">The type of data to use</typeparam>
        /// <param name="vertices">Array of vertex data</param>
        public void SetData<T>(T[] vertices) where T : struct
        {
            var t = typeof(T);

            // Calculate size metrics of the data
            var tSize = Marshal.SizeOf(t);

            if (t == typeof(Vector2) || t == typeof(Vector2d) || t == typeof(Vector2h)) {
                _unitSize = tSize / 2;
            } else if (t == typeof(Vector3) || t == typeof(Vector3d) || t == typeof(Vector3h)) {
                _unitSize = tSize / 3;
            } else if (t == typeof(Vector4) || t == typeof(Vector4d) || t == typeof(Vector4h)) {
                _unitSize = tSize / 4;
            } else {
                _unitSize = tSize;
            }

            _dataLength = (vertices.Length * tSize) / (Stride * _unitSize);

            // Bind the VBO, populate it, then unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * tSize), vertices, _usageHint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Check that nothing went wrong
            Tools.ErrorCheck("setdata");

            // Record that the VBO now has data, and may be drawn from
            _dataSet = true;
        }

        /// <summary>
        /// Prepare to draw from the VBO using a given shader.
        /// </summary>
        /// <param name="shader">Shader to use when drawing from the VBO</param>
        public virtual void Begin(ShaderProgram shader)
        {
            // Bind the buffer ready for drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            
            _curShader = shader;

            // Prepare the shader for drawing, and set up attribute pointers
            shader.Begin(false);
        }

        /// <summary>
        /// Draw a range of vertices from the VBO.
        /// </summary>
        /// <param name="first">Index of the first vertex to draw</param>
        /// <param name="count">Number of vertices to draw</param>
        public virtual void Render(int first = 0, int count = -1)
        {
            // Don't try and draw if the VBO hasn't been populated
            if (DataSet) {
                // If no count is specified, draw all vertices
                if (count == -1) {
                    count = _dataLength - first;
                }

                // Draw the specified range of vertices
                GL.DrawArrays(_curShader.PrimitiveType, first, count);
            }
        }

        /// <summary>
        /// Finish drawing from the VBO.
        /// </summary>
        public virtual void End()
        {
            // Unbind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Disable VBO drawing in the shader
            _curShader.End();
        }

        /// <summary>
        /// Dispose of unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            // If the VBO has been created, delete it
            if (_vboID != 0) {
                GL.DeleteBuffer(_vboID);
                _vboID = 0;
            }

            _dataSet = false;
        }
    }
}
