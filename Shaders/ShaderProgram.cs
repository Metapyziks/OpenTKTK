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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using OpenTKTK.Textures;
using OpenTKTK.Utils;

namespace OpenTKTK.Shaders
{
    public class ShaderProgram : IDisposable
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

        public class AttributeCollection : IEnumerable<AttributeInfo>
        {
            private static int GetAttributeSize(ShaderVarType type)
            {
                switch (type) {
                    case ShaderVarType.Float:
                    case ShaderVarType.Int:
                        return 1;
                    case ShaderVarType.Vec2:
                        return 2;
                    case ShaderVarType.Vec3:
                        return 3;
                    case ShaderVarType.Vec4:
                        return 4;
                    default:
                        throw new ArgumentException("Invalid attribute type (" + type + ").");
                }
            }

            private ShaderProgram _shader;

            internal AttributeCollection(ShaderProgram shader)
            {
                _shader = shader;
            }

            public AttributeInfo this[int index]
            {
                get { return _shader._attributes[index]; }
            }

            public AttributeInfo this[String ident]
            {
                get { return _shader._attributes.First(x => x.Identifier == ident); }
            }

            public IEnumerator<AttributeInfo> GetEnumerator()
            {
                return _shader._attributes.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return _shader._attributes.GetEnumerator();
            }
        }

        private static ShaderProgram _sCurProgram;
        
        public int VertexDataStride { get; private set; }
        public int VertexDataSize { get; private set; }

        private List<AttributeInfo> _attributes;
        private Dictionary<String, TextureInfo> _textures;
        private Dictionary<String, int> _uniforms;

        public int Program { get; private set; }

        public PrimitiveType PrimitiveType { get; protected set; }

        public bool Flat { get; private set; }

        public bool Active
        {
            get { return _sCurProgram == this; }
        }

        public bool Immediate { get; protected set; }
        public bool Started { get; protected set; }

        public AttributeCollection Attributes { get; private set; }

        public ShaderProgram(bool flat)
        {
            PrimitiveType = PrimitiveType.Triangles;
            Flat = flat;

            _attributes = new List<AttributeInfo>();
            _textures = new Dictionary<String, TextureInfo>();
            _uniforms = new Dictionary<String, int>();

            VertexDataStride = 0;
            VertexDataSize = 0;

            Attributes = new AttributeCollection(this);

            Started = false;

            Create();
        }

        protected virtual void ConstructVertexShader(ShaderBuilder vert)
        {
            return;
        }

        protected virtual void ConstructFragmentShader(ShaderBuilder frag)
        {
            return;
        }

        private void Create()
        {
            Program = GL.CreateProgram();

            int vert = GL.CreateShader(ShaderType.VertexShader);
            int frag = GL.CreateShader(ShaderType.FragmentShader);

            var vertBuilder = new ShaderBuilder(ShaderType.VertexShader, Flat);
            ConstructVertexShader(vertBuilder);

            var fragBuilder = new ShaderBuilder(ShaderType.FragmentShader, Flat, vertBuilder);
            ConstructFragmentShader(fragBuilder);

            GL.ShaderSource(vert, vertBuilder.Generate());
            GL.ShaderSource(frag, fragBuilder.Generate());

            GL.CompileShader(vert);
            GL.CompileShader(frag);

            String log;
            Trace.WriteLine(GetType().FullName);
            if ((log = GL.GetShaderInfoLog(vert).Trim()).Length > 0) Trace.WriteLine(log);
            if ((log = GL.GetShaderInfoLog(frag).Trim()).Length > 0) Trace.WriteLine(log);

            GL.AttachShader(Program, vert);
            GL.AttachShader(Program, frag);

            GL.LinkProgram(Program);

            if ((log = GL.GetProgramInfoLog(Program).Trim()).Length > 0) Trace.WriteLine(log);
            Trace.WriteLine("----------------");
            Use();

            if (Tools.GL3) {
                GL.BindFragDataLocation(Program, 0, "out_colour");
            }

            foreach (var uniform in vertBuilder.Uniforms.Union(fragBuilder.Uniforms)) {
                switch (uniform.Value) {
                    case ShaderVarType.Sampler2D:
                    case ShaderVarType.Sampler2DArray:
                    case ShaderVarType.SamplerCube:
                        AddTexture(uniform.Key);
                        break;
                    default:
                        AddUniform(uniform.Key);
                        break;
                }
            }

            OnCreate();

            Tools.ErrorCheck("create");
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

            Tools.ErrorCheck("addattrib:" + identifier);
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

            Tools.ErrorCheck("addtexture");
        }

        public void SetTexture(String identifier, Texture texture)
        {
            if (Started && Immediate) {
                GL.End();
                Tools.ErrorCheck("end");
            }

            _textures[identifier].SetCurrentTexture(texture);

            Tools.ErrorCheck("settexture");

            if (Started && Immediate)
                GL.Begin(PrimitiveType);
        }

        protected int GetUniformLocation(String identifier)
        {
            return GL.GetUniformLocation(Program, identifier);
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
            Immediate = immediateMode;
            Started = true;

            Use();
            OnBegin();

            Tools.ErrorCheck("begin");

            if (immediateMode) {
                GL.Begin(PrimitiveType);
            } else {
                foreach (AttributeInfo info in _attributes) {
                    GL.VertexAttribPointer(info.Location, info.Size,
                        info.PointerType, info.Normalize, VertexDataStride, info.Offset);

                    GL.EnableVertexAttribArray(info.Location);
                }
            }
        }

        protected virtual void OnBegin() { }

        public void End()
        {
            Started = false;

            if (Immediate) {
                GL.End();
            } else {
                foreach (AttributeInfo info in _attributes) {
                    GL.DisableVertexAttribArray(info.Location);
                }
            }

            OnEnd();

            Tools.ErrorCheck("end");
        }

        protected virtual void OnEnd() { }

        public virtual void Render(float[] data)
        {
            if (!Started) {
                throw new Exception("Must call Begin() first!");
            }

            if (!Immediate) {
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

        public virtual void Dispose()
        {
            if (Program != 0) {
                GL.DeleteProgram(Program);
            }
        }
    }
}
