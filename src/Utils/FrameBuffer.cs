using System;

using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;

namespace ComputerGraphicsCoursework.Utils
{
    /// <summary>
    /// Class that creates and manages an OpenGL frame buffer object (FBO).
    /// </summary>
    public sealed class FrameBuffer : IDisposable
    {
        #region Private Fields
        private int _fboID;
        #endregion

        /// <summary>
        /// Identification number assigned by OpenGL when the FBO is created.
        /// </summary>
        public int FboID
        {
            get
            {
                // If the FBO doesn't exist yet, create it
                if (_fboID == 0) _fboID = GL.GenFramebuffer();

                return _fboID;
            }
        }

        /// <summary>
        /// Gets the texture the FBO will draw to.
        /// </summary>
        public Texture Texture { get; private set; }
        
        /// <summary>
        /// Constructor to create a new FrameBuffer instance.
        /// </summary>
        /// <param name="tex">Texture the frame buffer will write to</param>
        public FrameBuffer(Texture tex)
        {
            Texture = tex;

            // Prepare the target texture for use
            Texture.Bind();

            // Assign the texture to the frame buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tex.TextureTarget, tex.TextureID, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Tools.ErrorCheck("fbo_init");
        }

        /// <summary>
        /// Start using the frame buffer as a render target.
        /// </summary>
        public void Begin()
        {
            // Bind the frame buffer and set up the viewport
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboID);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.PushAttrib(AttribMask.ViewportBit);
            GL.Viewport(0, 0, Texture.Width, Texture.Height);
        }

        /// <summary>
        /// Finish using the frame buffer as a render target.
        /// </summary>
        public void End()
        {
            // Restore the original viewport and unbind the frame buffer
            GL.PopAttrib();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            // Tools.ErrorCheck("fbo_end");
        }

        /// <summary>
        /// Dispose of any unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_fboID != 0) {
                GL.DeleteFramebuffer(_fboID);
                _fboID = 0;
            }
        }
    }
}
