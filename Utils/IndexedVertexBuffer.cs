using System;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL;
using OpenTKTK.Shaders;

namespace OpenTKTK.Utils
{
    public class IndexedVertexBuffer : VertexBuffer
    {
        private int _indicesVboID;
        private int _indicesLength;
        private int _indicesSize;
        private bool _indicesSet;
        private DrawElementsType _indicesType;

        public int IndicesVboID
        {
            get
            {
                // If the VBO doesn't exist yet, create it
                if (_indicesVboID == 0) GL.GenBuffers(1, out _indicesVboID);

                return _indicesVboID;
            }
        }

        public override bool DataSet
        {
            get
            {
                return base.DataSet && _indicesSet;
            }
        }

        public IndexedVertexBuffer(int stride, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
            : base(stride, usageHint)
        {
            _indicesSet = false;
        }

        public void SetIndices<T>(T[] indices) where T : struct
        {
            var t = typeof(T);
            
            // Calculate size metrics of the data
            _indicesSize = Marshal.SizeOf(t);

            switch (_indicesSize) {
                case 1:
                    _indicesType = DrawElementsType.UnsignedByte;
                    break;
                case 2:
                    _indicesType = DrawElementsType.UnsignedShort;
                    break;
                case 4:
                    _indicesType = DrawElementsType.UnsignedInt;
                    break;
                default:
                    throw new ArgumentException("Invalid indices type");
            }

            _indicesLength = indices.Length;

            // Bind the VBO, populate it, then unbind
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(indices.Length * _indicesSize), indices, UsageHint);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            // Check that nothing went wrong
            Tools.ErrorCheck("setindices");

            // Record that the VBO now has indices, and may be drawn from
            _indicesSet = true;
        }

        public override void Begin(ShaderProgram shader)
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVboID);

            base.Begin(shader);
        }

        public override void Render(int first = 0, int count = -1)
        {
            // Don't try and draw if the VBOs haven't been populated
            if (DataSet) {
                // If no count is specified, draw all vertices
                if (count == -1) {
                    count = _indicesLength - first;
                }

                // Draw the specified range of vertices
                GL.DrawElements(CurrentShader.PrimitiveType, count, _indicesType, first * _indicesSize);
            }
        }

        public override void End()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            base.End();
        }

        public override void Dispose()
        {
            base.Dispose();

            if (_indicesVboID != 0) {
                GL.DeleteBuffer(_indicesVboID);
                _indicesVboID = 0;
            }

            _indicesSet = false;
        }
    }
}
