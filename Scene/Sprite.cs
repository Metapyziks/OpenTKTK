using System;

using OpenTK;
using OpenTK.Graphics;

using OpenTKTK.Textures;
using OpenTKTK.Shaders;

namespace OpenTKTK.Scene
{
    public class Sprite
    {
        internal float[] Vertices
        {
            get
            {
                return _vertices;
            }
        }

        private float[] _vertices;

        private Vector2 _position;
        private Vector2 _scale;

        private Vector2 _subrectOffset;
        private Vector2 _subrectSize;

        private bool _flipHorz;
        private bool _flipVert;

        private float _rotation;
        private bool _useCentreAsOrigin;
        private Color4 _colour;

        protected bool VertsChanged;

        public virtual Vector2 Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value != _position) {
                    _position = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 Size
        {
            get
            {
                return new Vector2(_subrectSize.X * Scale.X, _subrectSize.Y * Scale.Y);
            }
            set
            {
                Scale = new Vector2(value.X / _subrectSize.X, value.Y / _subrectSize.Y);
            }
        }

        public virtual Vector2 Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                if (value != _scale) {
                    _scale = value;
                    VertsChanged = true;
                }
            }
        }

        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector2(value, Y);
            }
        }
        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector2(X, value);
            }
        }

        public virtual Vector2 SubrectOffset
        {
            get
            {
                return _subrectOffset;
            }
            set
            {
                if (value != _subrectOffset) {
                    _subrectOffset = value;
                    VertsChanged = true;
                }
            }
        }

        public virtual Vector2 SubrectSize
        {
            get
            {
                return _subrectSize;
            }
            set
            {
                if (value != _subrectSize) {
                    _subrectSize = value;
                    VertsChanged = true;
                }
            }
        }

        public float SubrectLeft
        {
            get
            {
                return SubrectOffset.X;
            }
            set
            {
                SubrectOffset = new Vector2(value, SubrectTop);
            }
        }

        public float SubrectTop
        {
            get
            {
                return SubrectOffset.Y;
            }
            set
            {
                SubrectOffset = new Vector2(SubrectLeft, value);
            }
        }

        public float SubrectRight
        {
            get
            {
                return SubrectOffset.X + SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2(value - SubrectOffset.X, SubrectHeight);
            }
        }

        public float SubrectBottom
        {
            get
            {
                return SubrectOffset.Y + SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2(SubrectWidth, value - SubrectOffset.Y);
            }
        }

        public float SubrectWidth
        {
            get
            {
                return SubrectSize.X;
            }
            set
            {
                SubrectSize = new Vector2(value, SubrectHeight);
            }
        }

        public float SubrectHeight
        {
            get
            {
                return SubrectSize.Y;
            }
            set
            {
                SubrectSize = new Vector2(SubrectWidth, value);
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }
            set
            {
                Scale = new Vector2(value / SubrectSize.X, Scale.Y);
            }
        }
        public float Height
        {
            get
            {
                return Size.Y;
            }
            set
            {
                Scale = new Vector2(Scale.X, value / SubrectSize.Y);
            }
        }

        public bool FlipHorizontal
        {
            get
            {
                return _flipHorz;
            }
            set
            {
                if (value != _flipHorz) {
                    _flipHorz = value;
                    VertsChanged = true;
                }
            }
        }

        public bool FlipVertical
        {
            get
            {
                return _flipVert;
            }
            set
            {
                if (value != _flipVert) {
                    _flipVert = value;
                    VertsChanged = true;
                }
            }
        }

        public float Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                if (value != _rotation) {
                    _rotation = value;
                    VertsChanged = true;
                }
            }
        }

        public bool UseCentreAsOrigin
        {
            get
            {
                return _useCentreAsOrigin;
            }
            set
            {
                if (value != _useCentreAsOrigin) {
                    _useCentreAsOrigin = value;
                    VertsChanged = true;
                }
            }
        }

        public Color4 Colour
        {
            get
            {
                return _colour;
            }
            set
            {
                if (value != _colour) {
                    _colour = value;
                    VertsChanged = true;
                }
            }
        }

        public BitmapTexture2D Texture { get; protected set; }

        public Sprite(float width, float height, Color4 colour)
        {
            Texture = BitmapTexture2D.Blank;

            Position = new Vector2();
            Scale = new Vector2(width, height);
            SubrectOffset = new Vector2(0, 0);
            SubrectSize = new Vector2(Texture.Width, Texture.Height);
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = colour;
        }

        public Sprite(BitmapTexture2D texture, float scale = 1.0f)
        {
            Texture = texture;

            Position = new Vector2();
            SubrectOffset = new Vector2(0, 0);
            SubrectSize = new Vector2(Texture.Width, Texture.Height);
            FlipHorizontal = false;
            FlipVertical = false;
            Rotation = 0;
            UseCentreAsOrigin = false;
            Colour = new Color4(1.0f, 1.0f, 1.0f, 1.0f);

            Scale = new Vector2(scale, scale);
        }

        protected virtual float[] FindVerts()
        {
            Vector2 tMin = Texture.GetCoords(SubrectLeft, SubrectTop);
            Vector2 tMax = Texture.GetCoords(SubrectRight, SubrectBottom);
            float xMin = FlipHorizontal ? tMax.X : tMin.X;
            float yMin = FlipVertical ? tMax.Y : tMin.Y;
            float xMax = FlipHorizontal ? tMin.X : tMax.X;
            float yMax = FlipVertical ? tMin.Y : tMax.Y;

            float halfWid = Width / 2;
            float halfHei = Height / 2;

            float[,] verts = UseCentreAsOrigin ? new float[,]
            {
                { -halfWid, -halfHei },
                { +halfWid, -halfHei },
                { +halfWid, +halfHei },
                { -halfWid, +halfHei }
            } : new float[,]
            {
                { 0, 0 },
                { Width, 0 },
                { Width, Height },
                { 0, Height }
            };

            float[,] mat = new float[,]
            {
                { (float) Math.Cos( Rotation ), -(float) Math.Sin( Rotation ) },
                { (float) Math.Sin( Rotation ),  (float) Math.Cos( Rotation ) }
            };

            for (int i = 0; i < 4; ++i) {
                float x = verts[i, 0];
                float y = verts[i, 1];
                verts[i, 0] = X + mat[0, 0] * x + mat[0, 1] * y;
                verts[i, 1] = Y + mat[1, 0] * x + mat[1, 1] * y;
            }

            return new float[]
            {
                verts[0, 0], verts[0, 1], xMin, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[1, 0], verts[1, 1], xMax, yMin, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[2, 0], verts[2, 1], xMax, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
                verts[3, 0], verts[3, 1], xMin, yMax, Colour.R, Colour.G, Colour.B, Colour.A,
            };
        }

        public virtual void Render(SpriteShader shader)
        {
            if (VertsChanged) {
                _vertices = FindVerts();
                VertsChanged = false;
            }

            shader.Texture = Texture;
            shader.Render(_vertices);
        }
    }
}
