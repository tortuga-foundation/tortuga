using Tortuga.Graphics.API;
using Tortuga.Utils.SDL2;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This contains global objects used by Tortuga.Graphics
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        internal GraphicsService GraphicsService => _graphicsService;
        private GraphicsService _graphicsService;

        /// <summary>
        /// runs on engine close
        /// </summary>
        public override void Destroy()
        {
        }

        /// <summary>
        /// runs on engine start
        /// </summary>
        public override void Init()
        {
            _graphicsService = new GraphicsService();
        }

        /// <summary>
        /// runs once per frame
        /// </summary>
        public override void Update()
        {
        }
    }
}