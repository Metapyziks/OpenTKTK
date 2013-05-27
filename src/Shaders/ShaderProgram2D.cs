using ComputerGraphicsCoursework.Utils;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ShaderProgram2D : ShaderProgram
    {
        public ShaderProgram2D()
            : base()
        {

        }

        public ShaderProgram2D(int width, int height)
            : base()
        {
            Create();
            SetScreenSize(width, height);
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddUniform("screen_resolution");
        }

        public void SetScreenSize(int width, int height)
        {
            SetUniform("screen_resolution", (float) width, (float) height);

            // Tools.ErrorCheck("screensize");
        }
    }
}
