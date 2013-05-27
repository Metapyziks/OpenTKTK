using System;
using System.Diagnostics;

using OpenTK.Graphics.OpenGL;

namespace ComputerGraphicsCoursework.Utils
{
    public static class Tools
    {
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
                var trace = new StackTrace();
                Debug.WriteLine(ec.ToString() + " at " + loc + Environment.NewLine + trace.ToString());
                throw new Exception(ec.ToString() + " at " + loc);
            }
#endif
        }
    }
}
