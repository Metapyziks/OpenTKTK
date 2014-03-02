using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTKTK.Scene;
using OpenTKTK.Shaders;
using OpenTKTK.Textures;
using OpenTKTK.Utils;

namespace OpenTKTK.Scene
{
    public class Text : Sprite
    {
        private static readonly Brush _brush = new SolidBrush(Color.White);

        private Font _font;
        private String _value;
        private bool _invalidated;

        public Font Font
        {
            get { return _font; }
            set
            {
                if (_font != value) {
                    _font = value;
                    Invalidate();
                }
            }
        }

        public String Value
        {
            get { return _value; }
            set
            {
                if (_value != value) {
                    _value = value;
                    Invalidate();
                }
            }
        }

        public Text(Font font, float scale = 1f) : base(0, 0, Color.White)
        {
            Value = String.Empty;
            Scale = new Vector2(scale, scale);
            Font = font;
            Invalidate();
        }

        private void Invalidate()
        {
            _invalidated = true;
        }

        protected virtual void UpdateImage()
        {
            using (var ctx = Graphics.FromImage(BitmapTexture2D.Blank.Bitmap)) {
                var size = ctx.MeasureString(Value, Font);
                SubrectWidth = (float) Math.Ceiling(size.Width);
                SubrectHeight = (float) Math.Ceiling(size.Height);
            }

            if (Texture.Bitmap.Width < SubrectWidth || Texture.Bitmap.Height < SubrectHeight) {
                if (Texture != BitmapTexture2D.Blank) Texture.Dispose();

                int newWidth = Math.Max((int) SubrectWidth, Texture.Bitmap.Width);
                int newHeight = Math.Max((int) SubrectHeight, Texture.Bitmap.Height);

                Texture = new BitmapTexture2D(MathHelper.NextPowerOfTwo(newWidth), MathHelper.NextPowerOfTwo(newHeight));
            }

            using (var ctx = Graphics.FromImage(Texture.Bitmap)) {
                ctx.SmoothingMode = SmoothingMode.HighQuality;
                ctx.Clear(Color.Transparent);

                var path = new GraphicsPath();

                path.AddString(Value, Font.FontFamily, (int) Font.Style,
                    ctx.DpiY * Font.Size / 72f, PointF.Empty, StringFormat.GenericDefault);

                ctx.FillPath(_brush, path);
            }

            Texture.Invalidate();
        }

        public override void Render(SpriteShader shader)
        {
            if (_invalidated) {
                UpdateImage();
                _invalidated = false;
            }

            base.Render(shader);
        }
    }
}
