using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using ComputerGraphicsCoursework.Textures;
using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ShaderProgram
    {
        public class AttributeInfo
        {
            public ShaderProgram Shader { get; private set; }
            public String Identifier { get; private set; }
            public int Location { get; private set; }
            public int Size { get; private set; }
            public int Offset { get; private set; }
            public int Divisor { get; private set; }
            public int InputOffset { get; private set; }
            public VertexAttribPointerType PointerType { get; private set; }
            public bool Normalize { get; private set; }

            public int Length
            {
                get
                {
                    switch (PointerType) {
                        case VertexAttribPointerType.Byte:
                        case VertexAttribPointerType.UnsignedByte:
                            return Size * sizeof(byte);

                        case VertexAttribPointerType.Short:
                        case VertexAttribPointerType.UnsignedShort:
                            return Size * sizeof(short);

                        case VertexAttribPointerType.Int:
                        case VertexAttribPointerType.UnsignedInt:
                            return Size * sizeof(int);

                        case VertexAttribPointerType.HalfFloat:
                            return Size * sizeof(float) / 2;

                        case VertexAttribPointerType.Float:
                            return Size * sizeof(float);

                        case VertexAttribPointerType.Double:
                            return Size * sizeof(double);

                        default:
                            return 0;
                    }
                }
            }

            public AttributeInfo(ShaderProgram shader, String identifier,
                int size, int offset, int divisor, int inputOffset,
                VertexAttribPointerType pointerType =
                    VertexAttribPointerType.Float,
                bool normalize = false)
            {
                Shader = shader;
                Identifier = identifier;
                Location = GL.GetAttribLocation(shader.Program, Identifier);
                Size = size;
                Offset = offset;
                Divisor = divisor;
                InputOffset = inputOffset;
                PointerType = pointerType;
                Normalize = normalize;
            }

            public override String ToString()
            {
                return Identifier + " @" + Location + ", Size: " + Size + ", Offset: " + Offset;
            }
        }

        private class TextureInfo
        {
            public ShaderProgram Shader { get; private set; }
            public String Identifier { get; private set; }
            public int UniformLocation { get; private set; }
            public TextureUnit TextureUnit { get; private set; }
            public Texture CurrentTexture { get; private set; }

            public TextureInfo(ShaderProgram shader, String identifier,
                TextureUnit textureUnit = TextureUnit.Texture0)
            {
                Shader = shader;
                Identifier = identifier;
                UniformLocation = GL.GetUniformLocation(Shader.Program, Identifier);
                TextureUnit = textureUnit;

                Shader.Use();

                int val = (int) TextureUnit - (int) TextureUnit.Texture0;

                GL.Uniform1(UniformLocation, val);

                CurrentTexture = null;
            }

            public void SetCurrentTexture(Texture texture)
            {
                CurrentTexture = texture;

                GL.ActiveTexture(TextureUnit);
                CurrentTexture.Bind();
            }
        }

        private static bool _sVersionChecked;
        private static bool _sGL3;
        private static bool _sNVidiaCard = false;

        private static ShaderProgram _sCurProgram;

        public static bool GL3
        {
            get
            {
                if (!_sVersionChecked)
                    CheckGLVersion();

                return _sGL3;
            }
        }

        public static bool NVidiaCard
        {
            get
            {
                if (!_sVersionChecked)
                    CheckGLVersion();

                return _sNVidiaCard;
            }
        }

        private static void CheckGLVersion()
        {
            String _sr = GL.GetString(StringName.Version);
            _sGL3 = _sr.StartsWith("3.") || _sr.StartsWith("4.");

            _sr = GL.GetString(StringName.Vendor);
            _sNVidiaCard = _sr.ToUpper().StartsWith("NVIDIA");

            _sVersionChecked = true;
        }

        public int VertexDataStride;
        public int VertexDataSize;

        private List<AttributeInfo> _attributes;
        private Dictionary<String, TextureInfo> _textures;
        private Dictionary<String, int> _uniforms;

        private bool _immediate;
        private bool _started;

        public int Program { get; private set; }

        public BeginMode BeginMode;
        public String VertexSource;
        public String FragmentSource;

        public bool Active
        {
            get { return _sCurProgram == this; }
        }

        public ShaderProgram()
        {
            BeginMode = BeginMode.Triangles;
            _attributes = new List<AttributeInfo>();
            _textures = new Dictionary<String, TextureInfo>();
            _uniforms = new Dictionary<String, int>();
            VertexDataStride = 0;
            VertexDataSize = 0;
            _started = false;
        }

        public void Create()
        {
            Program = GL.CreateProgram();

            int vert = GL.CreateShader(ShaderType.VertexShader);
            int frag = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vert, VertexSource);
            GL.ShaderSource(frag, FragmentSource);

            GL.CompileShader(vert);
            GL.CompileShader(frag);
#if DEBUG
            String log;
            Debug.WriteLine(GetType().FullName);
            if ((log = GL.GetShaderInfoLog(vert).Trim()).Length > 0) Debug.WriteLine(log);
            if ((log = GL.GetShaderInfoLog(frag).Trim()).Length > 0) Debug.WriteLine(log);
#endif

            GL.AttachShader(Program, vert);
            GL.AttachShader(Program, frag);

            GL.LinkProgram(Program);
#if DEBUG
            if ((log = GL.GetProgramInfoLog(Program).Trim()).Length > 0) Debug.WriteLine(log);
            Debug.WriteLine("----------------");
#endif
            Use();

            if (GL3) {
                GL.BindFragDataLocation(Program, 0, "out_colour");
            }

            OnCreate();

            // Tools.ErrorCheck("create");
        }

        protected virtual void OnCreate()
        {
            return;
        }

        public void Use()
        {
            if (!Active) {
                _sCurProgram = this;
                GL.UseProgram(Program);
            }
        }

        protected void AddAttribute(String identifier, int size, int divisor = 0, int inputOffset = -1,
            VertexAttribPointerType pointerType = VertexAttribPointerType.Float,
            bool normalize = false)
        {
            if (inputOffset == -1) {
                inputOffset = VertexDataSize;
            }

            AttributeInfo info = new AttributeInfo(this, identifier, size, VertexDataStride,
                divisor, inputOffset - VertexDataSize, pointerType, normalize);

            VertexDataStride += info.Length;
            VertexDataSize += info.Size;
            _attributes.Add(info);

            // Tools.ErrorCheck("addattrib:" + identifier);
        }

        protected void AddUnusedAttribute(int size, VertexAttribPointerType pointerType = VertexAttribPointerType.Float)
        {
            AttributeInfo info = new AttributeInfo(this, String.Empty, size, VertexDataStride,
                0, 0, pointerType, false);

            VertexDataStride += info.Length;
            VertexDataSize += info.Size;
        }

        protected void AddTexture(String identifier)
        {
            _textures.Add(identifier, new TextureInfo(this, identifier,
                (TextureUnit) Enumerable.Range((int) TextureUnit.Texture0, 16).First(x =>
                    _textures.Count(y => y.Value.TextureUnit == (TextureUnit) x) == 0)));

            // Tools.ErrorCheck("addtexture");
        }

        public void SetTexture(String identifier, Texture texture)
        {
            if (_started && _immediate) {
                GL.End();
                // Tools.ErrorCheck("end");
            }

            _textures[identifier].SetCurrentTexture(texture);

            // Tools.ErrorCheck("settexture");

            if (_started && _immediate)
                GL.Begin(BeginMode);
        }

        protected void AddUniform(String identifier)
        {
            _uniforms.Add(identifier, GL.GetUniformLocation(Program, identifier));
        }

        public void SetUniform(String identifier, int value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform1(loc, value);
        }

        public void SetUniform(String identifier, float value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform1(loc, value);
        }

        public void SetUniform(String identifier, Vector2 value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform2(loc, value);
        }

        public void SetUniform(String identifier, float x, float y)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform2(loc, x, y);
        }

        public void SetUniform(String identifier, Vector3 value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform3(loc, value);
        }

        public void SetUniform(String identifier, Vector4 value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform4(loc, value);
        }

        public void SetUniform(String identifier, Color4 value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.Uniform4(loc, value);
        }

        public void SetUniform(String identifier, ref Matrix4 value)
        {
            if (!Active) Use(); int loc = _uniforms[identifier]; if (loc == -1) return;
            GL.UniformMatrix4(loc, false, ref value);
        }

        public void Begin(bool immediateMode)
        {
            Use();
            OnBegin();

            // Tools.ErrorCheck("begin");

            if (immediateMode) {
                GL.Begin(BeginMode);
            } else {
                foreach (AttributeInfo info in _attributes) {
                    GL.VertexAttribPointer(info.Location, info.Size,
                        info.PointerType, info.Normalize, VertexDataStride, info.Offset);

                    GL.EnableVertexAttribArray(info.Location);
                }
            }

            _immediate = immediateMode;
            _started = true;
        }

        protected virtual void OnBegin() { }

        public void End()
        {
            _started = false;

            if (_immediate) {
                GL.End();
            } else {
                foreach (AttributeInfo info in _attributes) {
                    GL.DisableVertexAttribArray(info.Location);
                }
            }

            OnEnd();

            // Tools.ErrorCheck("end");
        }

        protected virtual void OnEnd() { }

        public virtual void Render(float[] data)
        {
            if (!_started) {
                throw new Exception("Must call Begin() first!");
            }

            if (!_immediate) {
                throw new Exception("Must use immediate mode!");
            }

            int i = 0;
            while (i < data.Length) {
                foreach (AttributeInfo attr in _attributes) {
                    int offset = attr.InputOffset;

                    switch (attr.Size) {
                        case 1:
                            GL.VertexAttrib1(attr.Location,
                                data[i++ + offset]);
                            break;
                        case 2:
                            GL.VertexAttrib2(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                        case 3:
                            GL.VertexAttrib3(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                        case 4:
                            GL.VertexAttrib4(attr.Location,
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset],
                                data[i++ + offset]);
                            break;
                    }
                }
            }
        }
    }
}
