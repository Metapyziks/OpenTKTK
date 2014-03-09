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
using OpenTKTK.Textures;

namespace OpenTKTK.Utils
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

        private int _depthBufferID;

        /// <summary>
        /// Gets the texture the FBO will draw to.
        /// </summary>
        public Texture Texture { get; private set; }
        
        /// <summary>
        /// Constructor to create a new FrameBuffer instance.
        /// </summary>
        /// <param name="tex">Texture the frame buffer will write to</param>
        public FrameBuffer(Texture tex, int depthBits = 0)
        {
            Texture = tex;

            // Prepare the target texture for use
            Texture.Bind();

            if (depthBits > 0) {
                _depthBufferID = GL.GenRenderbuffer();

                RenderbufferStorage rbStorage;
                switch (depthBits) {
                    case 16:
                        rbStorage = RenderbufferStorage.DepthComponent16; break;
                    case 24:
                        rbStorage = RenderbufferStorage.DepthComponent24; break;
                    case 32:
                        rbStorage = RenderbufferStorage.DepthComponent32; break;
                    default:
                        throw new ArgumentException("Invalid depth buffer bit count.");
                }

                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthBufferID);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, rbStorage, tex.Width, tex.Height);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
            }

            // Assign the texture to the frame buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FboID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, tex.TextureTarget, tex.TextureID, 0);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthBufferID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            Tools.ErrorCheck("fbo_init");
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
