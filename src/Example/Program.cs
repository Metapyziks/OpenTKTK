using System;
using System.Diagnostics;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKTK.Scene;
using OpenTKTK.Shaders;

namespace Example
{
    class Program : GameWindow
    {
        private SpriteShader _spriteShader;
        private Text _testText;

        private Stopwatch _timer;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            VSync = VSyncMode.Off;

            _timer = new Stopwatch();
            _timer.Start();

            _spriteShader = new SpriteShader(Width, Height);
            _testText = new Text(new Font(FontFamily.GenericSansSerif, 32f)) {
                UseCentreAsOrigin = true,
                Colour = Color.CornflowerBlue,
                X = Width / 2f,
                Y = Height / 2f,
            };
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            _testText.Value = DateTime.Now.ToLongTimeString();
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            float t = (float) _timer.Elapsed.TotalSeconds * MathHelper.Pi;

            _testText.Rotation = t;

            _spriteShader.Begin(true);
            _testText.Render(_spriteShader);
            _spriteShader.End();

            SwapBuffers();
        }

        static void Main(string[] args)
        {
            using (var program = new Program()) program.Run();
        }
    }
}
