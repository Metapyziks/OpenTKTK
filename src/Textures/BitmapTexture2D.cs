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
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Utils;

namespace OpenTKTK.Textures
{
    /// <summary>
    /// Class representing a two dimensional texture of coloured pixels.
    /// </summary>
    public sealed class BitmapTexture2D : Texture
    {
        /// <summary>
        /// Default texture containing a single white pixel.
        /// </summary>
        public static readonly BitmapTexture2D Blank;

        /// <summary>
        /// Static constructor for BitmapTexture2D. Creates the default texture.
        /// </summary>
        static BitmapTexture2D()
        {
            Bitmap blankBmp = new Bitmap(1, 1);
            blankBmp.SetPixel(0, 0, Color.White);
            Blank = new BitmapTexture2D(blankBmp);
        }

        /// <summary>
        /// Loads an image from a given file path and loads it into
        /// a BitmapTexture2D.
        /// </summary>
        /// <param name="filePath">File path of the image to load</param>
        /// <returns>Image loaded into a BitmapTexture2D</returns>
        public static BitmapTexture2D FromFile(String filePath)
        {
            return new BitmapTexture2D(new Bitmap(filePath));
        }

        #region Private Fields
        private readonly int _actualSize;
        #endregion

        /// <summary>
        /// Local bitmap representation of the texture.
        /// </summary>
        public Bitmap Bitmap { get; private set; }

        public TextureMinFilter MinFilter { get; set; }

        public TextureMagFilter MagFilter { get; set; }

        /// <summary>
        /// Constructor to create a new BitmapTexture2D instance from a bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap to load into the texture.</param>
        public BitmapTexture2D(Bitmap bitmap)
            : base(TextureTarget.Texture2D, bitmap.Width, bitmap.Height)
        {
            MinFilter = TextureMinFilter.LinearMipmapNearest;
            MagFilter = TextureMagFilter.Nearest;

            // To be safe, always use a power of two width and height
            _actualSize = MathHelper.NextPowerOfTwo(Math.Max(bitmap.Width, bitmap.Height));

            // If the image is already of power of two size, don't bother resizing
            if (_actualSize == bitmap.Width && _actualSize == bitmap.Height) {
                Bitmap = bitmap;
            } else {
                // Otherwise, create a new bitmap of the correct size
                Bitmap = new Bitmap(_actualSize, _actualSize);

                // Set each pixel of the new bitmap with the corresponding
                // pixel of the original
                for (int x = 0; x < Width; ++x) {
                    for (int y = 0; y < Height; ++y) {
                        Bitmap.SetPixel(x, y, bitmap.GetPixel(x, y));
                    }
                }
            }
        }

        /// <summary>
        /// Constructor to create a new BitmapTexture2D instance from a given
        /// width and height.
        /// </summary>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        public BitmapTexture2D(int width, int height)
            : this(new Bitmap(width, height)) { }

        /// <summary>
        /// Finds the actual texel coordinates for images that are not a power-of-two
        /// size, and have therefore been expanded.
        /// </summary>
        /// <param name="x">Horizontal position of the texel</param>
        /// <param name="y">Vertical position of the texel</param>
        /// <returns></returns>
        public Vector2 GetCoords(float x, float y)
        {
            return new Vector2 {
                X = x / _actualSize,
                Y = y / _actualSize
            };
        }

        /// <summary>
        /// Load the texture into video memory.
        /// </summary>
        protected override void Load()
        {
            if (!Tools.GL3) {
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);
            }

            // Transfer from the bitmap buffer to video memory
            BitmapData data = Bitmap.LockBits(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Bitmap.Width, Bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            Bitmap.UnlockBits(data);

            // This probably doesn't belong here - set the texture
            // filter and edge wrap modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) MagFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int) TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);

            // Generate mipmap levels from the new texture
            if (Tools.GL3) {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            Tools.ErrorCheck("loadtexture");
        }

        public override void Dispose()
        {
            base.Dispose();
            Bitmap.Dispose();
        }
    }
}
