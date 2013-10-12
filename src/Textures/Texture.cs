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
    /// Class that creates and manages an OpenGL texture.
    /// </summary>
    public abstract class Texture : IDisposable
    {
        #region Private Static Fields
        private static Texture _sCurrentLoadedTexture;
        #endregion

        /// <summary>
        /// Gets the currently bound texture, if it exists.
        /// </summary>
        public static Texture Current
        {
            get { return _sCurrentLoadedTexture; }
        }

        #region Private Fields
        private int _id;
        private bool _loaded;
        #endregion

        /// <summary>
        /// The texture target type used by this texture.
        /// </summary>
        public TextureTarget TextureTarget { get; private set; }

        /// <summary>
        /// Identification number assigned by OpenGL when the texture is created.
        /// </summary>
        public int TextureID
        {
            get
            {
                // If the texture doesn't exist yet, create it
                if (_id == -1) GL.GenTextures(1, out _id);

                return _id;
            }
        }

        /// <summary>
        /// Width of the texture in pixels.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the texture in pixels.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Depth of the texture in pixels.
        /// </summary>
        public int Depth { get; private set; }

        public bool Dirty { get { return !_loaded; } }

        /// <summary>
        /// Constructor to create a new Texture instance.
        /// </summary>
        /// <param name="target">Texture target type used by the texture</param>
        /// <param name="width">Width of the texture in pixels</param>
        /// <param name="height">Height of the texture in pixels</param>
        /// <param name="depth">Depth of the texture in pixels</param>
        protected Texture(TextureTarget target, int width, int height, int depth = 1)
        {
            TextureTarget = target;

            Width = width;
            Height = height;
            Depth = depth;

            _id = -1;
            _loaded = false;
        }

        /// <summary>
        /// Mark the texture as being out of date and needing reloading.
        /// </summary>
        public void Invalidate()
        {
            _loaded = false;
        }

        /// <summary>
        /// When overriden in a subclass, will load the texture into video memory.
        /// </summary>
        protected abstract void Load();

        /// <summary>
        /// Prepares the texture for usage.
        /// </summary>
        public void Bind()
        {
            // If this texture isn't already bound, bind it
            if (_sCurrentLoadedTexture != this) {
                GL.BindTexture(TextureTarget, TextureID);
                _sCurrentLoadedTexture = this;
            }

            // Tools.ErrorCheck("bindtexture");

            // If the texture has changed since the last time it was bound,
            // upload the new data to video memory
            if (!_loaded) {
                Load();
                _loaded = true;
            }
        }

        /// <summary>
        /// Dispose of unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            // If the texture has been created, delete it
            if (_id != 0) {
                GL.DeleteTexture(_id);
                _id = 0;
            }

            _loaded = false;
        }
    }
}
