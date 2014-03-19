using OpenTK.Graphics.OpenGL;

using OpenTKTK.Shaders;
using OpenTKTK.Textures;
using OpenTKTK.Utils;

namespace OpenTKTK.Shaders
{
    public class SpriteShader : ShaderProgram2D
    {
        private BitmapTexture2D _texture;

        public BitmapTexture2D Texture
        {
            get
            {
                return _texture;
            }
            set
            {
                if (_texture != value || _texture.Dirty) {
                    SetTexture("sprite", value);
                    _texture = value;
                }
            }
        }

        public SpriteShader()
        {
            PrimitiveType = PrimitiveType.Quads;
        }

        protected override void ConstructVertexShader(ShaderBuilder vert)
        {
            base.ConstructVertexShader(vert);

            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.AddAttribute(ShaderVarType.Vec2, "in_texture");
            vert.AddAttribute(ShaderVarType.Vec4, "in_colour");
            vert.AddVarying(ShaderVarType.Vec2, "var_texture");
            vert.AddVarying(ShaderVarType.Vec4, "var_colour");
            vert.Logic = @"
                void main(void)
                {
                    var_texture = in_texture;
                    var_colour = in_colour;

                    gl_Position = in_position;
                }
            ";
        }

        protected override void ConstructFragmentShader(ShaderBuilder frag)
        {
            base.ConstructFragmentShader(frag);

            frag.AddUniform(ShaderVarType.Sampler2D, "sprite");
            frag.Logic = @"
                void main(void)
                {
                    vec4 clr = texture2D(sprite, var_texture) * var_colour;

                    if (clr.a != 0.0) {
                        out_colour = clr.rgba;
                    } else {
                        discard;
                    }
                }
            ";
        }

        public SpriteShader(int width, int height)
            : this()
        {
            SetScreenSize(width, height);
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            if (Tools.NVidiaCard) {
                AddAttribute("in_texture", 2, 0, 2);
                AddAttribute("in_colour", 4, 0, 4);
                AddAttribute("in_position", 2, 0, 0);
            } else {
                AddAttribute("in_position", 2);
                AddAttribute("in_texture", 2);
                AddAttribute("in_colour", 4);
            }
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            GL.Enable(EnableCap.Blend);

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        protected override void OnEnd()
        {
            base.OnEnd();

            GL.Disable(EnableCap.Blend);
        }
    }
}
