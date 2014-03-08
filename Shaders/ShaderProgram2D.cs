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

using OpenTKTK.Utils;
namespace OpenTKTK.Shaders
{
    public class ShaderProgram2D : ShaderProgram
    {
        public ShaderProgram2D()
            : base(true)
        {

        }

        public ShaderProgram2D(int width, int height)
            : base(true)
        {
            SetScreenSize(width, height);
        }

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        public void SetScreenSize(int width, int height)
        {
            SetUniform("screen_resolution", (float) width, (float) height);

            // Tools.ErrorCheck("screensize");
        }
    }
}
