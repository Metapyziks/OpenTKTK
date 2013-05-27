using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Textures
{
    /// <summary>
    /// Class that creates and manages an OpenGL texture.
    /// </summary>
    public abstract class Texture
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
    }
}
