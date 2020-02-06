namespace Tortuga.Systems
{
    public class RenderingSystem : Core.BaseSystem
    {
        public override void Update()
        {
            var components = MyScene.GetComponents<Components.Camera>();
        }
    }
}