using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    public class GraphicsModule : Core.BaseModule
    {
        internal GraphicsService GraphicsService => _graphicsService;
        private GraphicsService _graphicsService;

        public override void Destroy()
        {
        }

        public override void Init()
        {
            _graphicsService = new GraphicsService();
        }

        public override void Update()
        {
        }
    }
}