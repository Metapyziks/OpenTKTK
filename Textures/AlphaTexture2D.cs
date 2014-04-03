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
using OpenTK.Graphics.OpenGL;

namespace OpenTKTK.Textures
{
    /// <summary>
    /// Class representing a two dimensional texture of single precision floats.
    /// </summary>
    public sealed class AlphaTexture2D : Texture
    {
        #region Private Fields
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
            // Create local buffer for data
            _data = new float[height, width];

            // If a clear value was specified, set each pixel to that value
            if (clear != 0f) {
                for (int x = 0; x < Width; ++x) {
                    for (int y = 0; y < Height; ++y) {
                        _data[y, x] = clear;
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
            get { return _data[y, x]; }
            set
            {
                _data[y, x] = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Load the texture into video memory.
        /// </summary>
        protected override void Load()
        {
            // Transfer from the local buffer to video memory
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, Width, Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.Float, _data);

            // This probably doesn't belong here - set the texture
            // filter and edge wrap modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        }
    }
}
