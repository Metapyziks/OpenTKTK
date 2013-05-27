using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Shaders;

namespace ComputerGraphicsCoursework.Utils
{
    /// <summary>
    /// Class that creates and manages an OpenGL vertex buffer object (VBO). 
    /// </summary>
    public sealed class VertexBuffer : IDisposable
    {
        #region Private Fields
        private int _stride;
        private BufferUsageHint _usageHint;

        private int _unitSize;
        private int _vboID;
        private int _length;

        private bool _dataSet;
        private ShaderProgram _curShader;
        #endregion

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

        /// <summary>
        /// Number of floats per vertex in the VBO.
        /// </summary>
        public int Stride
        {
            get { return _stride; }
        }

        /// <summary>
        /// Constructor to create a new VertexBuffer instance.
        /// </summary>
        /// <param name="stride">The number of floats per vertex</param>
        public VertexBuffer(int stride, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
        {
            _stride = stride;
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
            // Calculate size metrics of the data
            _unitSize = Marshal.SizeOf(typeof(T));
            _length = vertices.Length / _stride;

            // Bind the VBO, populate it, then unbind
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(vertices.Length * _unitSize), vertices, _usageHint);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Check that nothing went wrong
            // Tools.ErrorCheck("setdata");

            // Record that the VBO now has data, and may be drawn to
            _dataSet = true;
        }

        /// <summary>
        /// Prepare to draw from the VBO using a given shader.
        /// </summary>
        /// <param name="shader">Shader to use when drawing from the VBO</param>
        public void Begin(ShaderProgram shader)
        {
            _curShader = shader;

            // Bind the buffer ready for drawing
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboID);

            // Prepare the shader for drawing, and set up attribute pointers
            shader.Begin(false);
        }

        /// <summary>
        /// Draw a range of vertices from the VBO.
        /// </summary>
        /// <param name="first">Index of the first vertex to draw</param>
        /// <param name="count">Number of vertices to draw</param>
        public void Render(int first = 0, int count = -1)
        {
            // Don't try and draw if the VBO hasn't been populated
            if (_dataSet) {
                // If no count is specified, draw all vertices
                if (count == -1) {
                    count = _length - first;
                }

                // Draw the specified range of vertices
                GL.DrawArrays(_curShader.BeginMode, first, count);
            }
        }

        /// <summary>
        /// Finish drawing from the VBO.
        /// </summary>
        public void End()
        {
            // Unbind the VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Disable VBO drawing in the shader
            _curShader.End();
        }

        /// <summary>
        /// Dispose of unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // If the VBO has been created, delete it
            if (_vboID != 0) {
                GL.DeleteBuffers(1, ref _vboID);
                _vboID = 0;
            }

            _dataSet = false;
        }
    }
}
