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
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;

namespace OpenTKTK.Utils
{
    /// <summary>
    /// An enumeration of GLSL variable types supported by the
    /// shader builder utility.
    /// </summary>
    public enum ShaderVarType
    {
        Int,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Sampler2D,
        SamplerCube,
        Sampler2DArray,
        Mat4
    }

    /// <summary>
    /// Helper class to construct shader programs that are automatically
    /// altered to support different versions of the OpenGL Shader Language.
    /// </summary>
    public sealed class ShaderBuilder
    {
        #region Private Struct ShaderVariable
        /// <summary>
        /// Structure mapping a shader variable identifier to its type.
        /// </summary>
        private struct ShaderVariable
        {
            /// <summary>
            /// Identifier of the shader variable.
            /// </summary>
            public String Identifier;

            /// <summary>
            /// GLSL type of the shader variable.
            /// </summary>
            public ShaderVarType Type;

            /// <summary>
            /// GLSL type of the shader variable formatted to be used
            /// in GLSL source code.
            /// </summary>
            public String TypeString
            {
                get
                {
                    String str = Type.ToString();

                    // Return the type name with the first
                    // character in lower case
                    return str[0].ToString().ToLower()
                        + str.Substring(1);
                }
            }
        }
        #endregion

        #region Private Fields
        private bool _twoDimensional;

        private List<String> _extensions;

        private List<ShaderVariable> _uniforms;
        private List<ShaderVariable> _attribs;
        private List<ShaderVariable> _varyings;
        #endregion

        public IEnumerable<KeyValuePair<String, ShaderVarType>> Uniforms
        {
            get { return _uniforms.Select(x => new KeyValuePair<String, ShaderVarType>(x.Identifier, x.Type)); }
        }

        public IEnumerable<KeyValuePair<String, ShaderVarType>> Attributes
        {
            get { return _attribs.Select(x => new KeyValuePair<String, ShaderVarType>(x.Identifier, x.Type)); }
        }

        /// <summary>
        /// Shader type to generate the source for. Currently only vertex
        /// and fragment shaders are supported.
        /// </summary>
        public ShaderType Type { get; private set; }

        /// <summary>
        /// The shader logic written in GLSL. Must contain a main() method.
        /// </summary>
        public String Logic { get; set; }

        /// <summary>
        /// In the case of a fragment shader, the identifier used for the colour
        /// output variable.
        /// </summary>
        public String FragOutIdentifier { get; set; }

        /// <summary>
        /// Constructor to create a new ShaderBuilder instance.
        /// </summary>
        /// <param name="type">The shader type to generate the source for</param>
        /// <param name="twoDimensional">If true, some helper code for two dimensional shaders will be included</param>
        /// <param name="parent">Previous shader in the pipeline (if any)</param>
        public ShaderBuilder(ShaderType type, bool twoDimensional, ShaderBuilder parent = null)
        {
            Type = type;
            _twoDimensional = twoDimensional;

            // Prepare an empty list of OpenGL extensions
            _extensions = new List<String>();

            // Set up variable lists
            _uniforms = new List<ShaderVariable>();
            _attribs = new List<ShaderVariable>();
            _varyings = new List<ShaderVariable>();

            if (twoDimensional) {
                AddUniform(ShaderVarType.Vec2, "screen_resolution");
            }

            // If the builder is given a parent, copy any outputs
            // from that shader as inputs for this one
            if (parent != null) {
                foreach (var vary in parent._varyings) {
                    AddVarying(vary.Type, vary.Identifier);
                }
            }

            Logic = "";

            // Default fragment colour output variable identifier
            FragOutIdentifier = "out_colour";
        }

        /// <summary>
        /// Add a uniform of a specified type to the shader.
        /// </summary>
        /// <param name="type">GLSL type of the uniform</param>
        /// <param name="identifier">Identifier name of the uniform</param>
        public void AddUniform(ShaderVarType type, String identifier)
        {
            // If the type is a Sampler2DArray, include the
            // relevant extension
            if (type == ShaderVarType.Sampler2DArray) {
                String ext = "GL_EXT_texture_array";
                if (!_extensions.Contains(ext))
                    _extensions.Add(ext);
            }

            _uniforms.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        /// <summary>
        /// Add an attribute of a specified type to the shader.
        /// </summary>
        /// <param name="type">GLSL type of the attribute</param>
        /// <param name="identifier">Identifier name of the attribute</param>
        public void AddAttribute(ShaderVarType type, String identifier)
        {
            _attribs.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        /// <summary>
        /// Add a varying value of a specified type to the shader.
        /// </summary>
        /// <param name="type">GLSL type of the varying</param>
        /// <param name="identifier">Identifier name of the varying</param>
        public void AddVarying(ShaderVarType type, String identifier)
        {
            _varyings.Add(new ShaderVariable { Type = type, Identifier = identifier });
        }

        /// <summary>
        /// Generate the platform specific GLSL code from the given parameters.
        /// </summary>
        /// <param name="gl3">If true, OpenGL 3.0 specific features will be used</param>
        /// <returns>GLSL source code ready to be compiled</returns>
        public String Generate()
        {
            StringBuilder sb = new StringBuilder();

            // Specify GLSL version
            sb.AppendFormat("#version {0}", Tools.GL3 ? "130" : "120");
            sb.AppendLine();

            // List each required extension, if any exist
            if (_extensions.Count != 0) {
                foreach (String ext in _extensions) {
                    sb.AppendFormat("#extension {0} : enable", ext);
                    sb.AppendLine();
                }
                sb.AppendLine();
            }

            // Specify float precision if using OpenGL 3.0
            if (Tools.GL3) {
                sb.AppendLine("precision highp float;");
                sb.AppendLine();
            }

            // List each uniform and its type
            foreach (ShaderVariable var in _uniforms) {
                sb.AppendFormat("uniform {0} {1};", var.TypeString, var.Identifier);
                sb.AppendLine();
            }

            // If there was at least one uniform, add a separating newline
            if (_uniforms.Count != 0) {
                sb.AppendLine();
            }

            // List each attribute and its type
            foreach (ShaderVariable var in _attribs) {
                sb.AppendFormat("{0} {1} {2};", Tools.GL3 ? "in" : "attribute", var.TypeString, var.Identifier);
                sb.AppendLine();
            }

            // If there was at least one attribute, add a separating newline
            if (_attribs.Count != 0) {
                sb.AppendLine();
            }

            // List each varying and its type
            foreach (ShaderVariable var in _varyings) {
                sb.AppendFormat("{0} {1} {2};", Tools.GL3 ? Type == ShaderType.VertexShader
                    ? "out" : "in" : "varying", var.TypeString, var.Identifier);
                sb.AppendLine();
            }

            // If there was at least one attribute, add a separating newline
            if (_varyings.Count != 0) {
                sb.AppendLine();
            }

            // If this is a fragment shader using OpenGL 3.0+, include the
            // custom fragment output colour identifier
            if (Tools.GL3 && Type == ShaderType.FragmentShader) {
                sb.AppendFormat("out vec4 {0};", FragOutIdentifier);
                sb.AppendLine();
                sb.AppendLine();
            }

            // Please ignore the next 26 lines, thanks
            int index = Logic.IndexOf("void") - 1;
            String indent = "";
            while (index >= 0 && Logic[index] == ' ')
                indent += Logic[index--];

            indent = new String(indent.Reverse().ToArray());

            String logic = indent.Length == 0 ? Logic.Trim() : Logic.Trim().Replace(indent, "");

            if (Type == ShaderType.FragmentShader) {
                if (Tools.GL3)
                    logic = logic.Replace("texture2DArray(", "texture(")
                        .Replace("textureCube(", "texture(")
                        .Replace("texture2D(", "texture(");
                else
                    logic = logic.Replace(FragOutIdentifier, "gl_FragColor");
            } else if (_twoDimensional) {
                logic = logic.Replace("gl_Position", "vec2 _pos_");
                index = logic.IndexOf("_pos_");
                index = logic.IndexOf(';', index) + 1;
                logic = logic.Insert(index, Environment.NewLine
                    + "    _pos_ -= screen_resolution / 2.0;" + Environment.NewLine
                    + "    _pos_.x /= screen_resolution.x / 2.0;" + Environment.NewLine
                    + "    _pos_.y /= -screen_resolution.y / 2.0;" + Environment.NewLine
                    + "    gl_Position = vec4( _pos_, 0.0, 1.0 );");
            }

            sb.AppendLine("#line 0");
            sb.Append(logic);

            // Return the completed shader source
            return sb.ToString();
        }
    }
}
