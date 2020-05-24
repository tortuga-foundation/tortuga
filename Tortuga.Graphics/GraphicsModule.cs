#pragma warning disable 1591

namespace Tortuga.Graphics
{
    /// <summary>
    /// Graphics module
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        public override void Destroy()
        {
        }

        public override void Init()
        {
            //initialize vulkan
            API.Handler.Init();
        }

        public override void Update()
        {
        }
    }
}