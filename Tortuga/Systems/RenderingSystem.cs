namespace Tortuga.Systems
{
    public class RenderingSystem : Core.BaseSystem
    {
        public RenderingSystem()
        {

        }
        ~RenderingSystem()
        {

        }

        public override void Update()
        {
            var cameras = MyScene.GetComponents<Components.Camera>();
            foreach (var camera in cameras)
            {
                
            }
        }
    }
}