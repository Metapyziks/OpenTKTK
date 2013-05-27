using OpenTK;

using ComputerGraphicsCoursework.Scene;

namespace ComputerGraphicsCoursework.Shaders
{
    public class ShaderProgram3D : ShaderProgram
    {
        public int ScreenWidth { get; private set; }
        public int ScreenHeight { get; private set; }

        public Camera Camera { get; set; }

        protected override void OnCreate()
        {
            base.OnCreate();

            AddUniform("vp_matrix");
        }

        protected override void OnBegin()
        {
            if (Camera != null) {
                Matrix4 viewMat = Camera.CombinedMatrix;
                SetUniform("vp_matrix", ref viewMat);
            }
        }
    }
}
