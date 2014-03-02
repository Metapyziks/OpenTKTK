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

namespace OpenTKTK.Shaders
{
    public class ShaderProgram3D<T> : ShaderProgram
        where T : Camera
    {
        public T Camera { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddUniform("vp_matrix");
        }

        protected override void OnBegin()
        {
            base.OnBegin();

            if (Camera != null) {
                Matrix4 viewMat = Camera.CombinedMatrix;
                SetUniform("vp_matrix", ref viewMat);
            }
        }
    }
}
