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
                    SetTexture("texture0", value);
                    _texture = value;
                }
            }
        }

        public SpriteShader()
        {
            ShaderBuilder vert = new ShaderBuilder(ShaderType.VertexShader, true);
            vert.AddAttribute(ShaderVarType.Vec2, "in_position");
            vert.AddAttribute(ShaderVarType.Vec2, "in_texture");
            vert.AddAttribute(ShaderVarType.Vec4, "in_colour");
            vert.AddVarying(ShaderVarType.Vec2, "var_texture");
            vert.AddVarying(ShaderVarType.Vec4, "var_colour");
            vert.Logic = @"
                void main( void )
                {
                    var_texture = in_texture;
                    var_colour = in_colour;

                    gl_Position = in_position;
                }
            ";

            ShaderBuilder frag = new ShaderBuilder(ShaderType.FragmentShader, true);
            frag.AddUniform(ShaderVarType.Sampler2D, "texture0");
            frag.AddVarying(ShaderVarType.Vec2, "var_texture");
            frag.AddVarying(ShaderVarType.Vec4, "var_colour");
            frag.FragOutIdentifier = "out_frag_colour";
            frag.Logic = @"
                void main( void )
                {
                    vec4 clr = texture2D( texture0, var_texture ) * var_colour;

                    if( clr.a != 0.0 )
                        out_frag_colour = clr.rgba;
                    else
                        discard;
                }
            ";

            VertexSource = vert.Generate();
            FragmentSource = frag.Generate();

            BeginMode = BeginMode.Quads;

            Create();
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

            AddTexture("texture0");
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
