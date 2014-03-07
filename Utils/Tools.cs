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
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

namespace OpenTKTK.Utils
{
    public static class Tools
    {
        private static bool _sVersionChecked;
        private static bool _sGL3;
        private static bool _sNVidiaCard = false;

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
            String str = GL.GetString(StringName.Version);
            _sGL3 = str.StartsWith("3.") || str.StartsWith("4.");

            Trace.WriteLine("GL Version: " + str);

            str = GL.GetString(StringName.Vendor);
            _sNVidiaCard = str.ToUpper().StartsWith("NVIDIA");

            Trace.WriteLine("Vendor: " + str);

            _sVersionChecked = true;
        }

        public static float NextSingle(this Random rand)
        {
            return (float) rand.NextDouble();
        }

        public static float NextSingle(this Random rand, float max)
        {
            return (float) (rand.NextDouble() * max);
        }

        public static float NextSingle(this Random rand, float min, float max)
        {
            return min + (float) (rand.NextDouble() * (max - min));
        }

        /// <summary>
        /// Restrain a value to be within a given range.
        /// </summary>
        /// <param name="val">Value to restrain</param>
        /// <param name="min">Minimum value of range</param>
        /// <param name="max">Maximum value of range</param>
        /// <returns>The given value restrained within the given range</returns>
        public static float Clamp(float val, float min, float max)
        {
            return val < min ? min : val > max ? max : val;
        }

        /// <summary>
        /// Restrain a value to be within a given range.
        /// </summary>
        /// <param name="val">Value to restrain</param>
        /// <param name="min">Minimum value of range</param>
        /// <param name="max">Maximum value of range</param>
        /// <returns>The given value restrained within the given range</returns>
        public static double Clamp(double val, double min, double max)
        {
            return val < min ? min : val > max ? max : val;
        }

        /// <summary>
        /// Calls GL.GetError(), and throws an exception if an OpenGL
        /// error had occurred.
        /// </summary>
        /// <param name="loc">A string identifier to help record where an error was found</param>
        public static void ErrorCheck(String loc = "unknown")
        {
#if DEBUG
            ErrorCode ec = GL.GetError();

            // If there has been an OpenGL error...
            if (ec != ErrorCode.NoError) {
                // Print the current call stack to the debug output,
                // then throw an exception
                var trace = new StackTrace(1);
                Trace.WriteLine(ec.ToString() + " at " + loc + Environment.NewLine + trace.ToString());
                throw new Exception(ec.ToString() + " at " + loc);
            }
#endif
        }
    }
}
