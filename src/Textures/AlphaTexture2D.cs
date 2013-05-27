using System;

using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Textures
{
    /// <summary>
    /// Class representing a two dimensional texture of single precision floats.
    /// </summary>
    public sealed class AlphaTexture2D : Texture
    {
        #region Private Fields
        private readonly int _actualSize;
        private float[,] _data;
        #endregion

        /// <summary>
        /// Constructor to create a new AlphaTexture2D instance.
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="clear">Default value for each pixel</param>
        public AlphaTexture2D(int width, int height, float clear = 0f)
            : base(TextureTarget.Texture2D, width, height)
        {
            // To be safe, always use a power of two width and height
            _actualSize = MathHelper.NextPowerOfTwo(Math.Max(width, height));

            // Create local buffer for data
            _data = new float[Width, Height];

            // If a clear value was specified, set each pixel to that value
            if (clear != 0f) {
                for (int x = 0; x < Width; ++x) {
                    for (int y = 0; y < Height; ++y) {
                        _data[x, y] = clear;
                    }
                }

                // Mark the texture for updating
                Invalidate();
            }
        }
        
        /// <summary>
        /// Gets or sets the value of a pixel.
        /// </summary>
        /// <param name="x">Horizontal position of the pixel</param>
        /// <param name="y">Vertical position of the pixel</param>
        /// <returns></returns>
        public float this[int x, int y]
        {
            get { return _data[x, y]; }
            set
            {
                _data[x, y] = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Load the texture into video memory.
        /// </summary>
        protected override void Load()
        {
            // Transfer from the local buffer to video memory
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, _actualSize, _actualSize, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, _data);

            // This probably doesn't belong here - set the texture
            // filter and edge wrap modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        }
    }
}
