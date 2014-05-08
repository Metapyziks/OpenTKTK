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

using OpenTK;

using OpenTKTK.Scene;
using OpenTKTK.Utils;

namespace OpenTKTK.Shaders
{
    public class ShaderProgram3D<T> : ShaderProgram
        where T : Camera
    {
        public T Camera { get; set; }

        public ShaderProgram3D()
            : base(false) { }

        protected override void ConstructVertexShader(ShaderBuilder vert)
        {
            base.ConstructVertexShader(vert);

            vert.AddUniform(ShaderVarType.Mat4, "proj");
            vert.AddUniform(ShaderVarType.Mat4, "view");
            vert.AddUniform(ShaderVarType.Vec3, "camera");
        }

        protected override void ConstructFragmentShader(ShaderBuilder frag)
        {
            base.ConstructVertexShader(frag);

            frag.AddUniform(ShaderVarType.Mat4, "proj");
            frag.AddUniform(ShaderVarType.Mat4, "view");
            frag.AddUniform(ShaderVarType.Vec3, "camera");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            if (Camera != null) {
                var proj = Camera.ProjectionMatrix;
                var view = Camera.ViewMatrix;
                SetUniform("proj", ref proj);
                SetUniform("view", ref view);
                SetUniform("camera", Camera.Position);
            }
        }
    }
}
