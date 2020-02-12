namespace Tortuga.Systems
{
    public class AutoCameraResolution : Core.BaseSystem
    {
        public float Scale = 1.0f;

        public AutoCameraResolution()
        {
            Engine.Instance.MainWindow.SdlHandle.Resized += OnWindowResized;
        }
        ~AutoCameraResolution()
        {
            Engine.Instance.MainWindow.SdlHandle.Resized -= OnWindowResized;
        }

        public void OnWindowResized()
        {
            var cameras = MyScene.GetComponents<Components.Camera>();
            foreach (var camera in cameras)
                camera.Resolution = new Math.IntVector2D
                {
                    x = System.Convert.ToInt32(Engine.Instance.MainWindow.Width * Scale),
                    y = System.Convert.ToInt32(Engine.Instance.MainWindow.height * Scale)
                };
        }

        public override void Update() { }
    }
}