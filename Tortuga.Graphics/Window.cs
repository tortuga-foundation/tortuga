using Tortuga.Graphics.API;
using Tortuga.Utils.SDL2;

namespace Tortuga.Graphics
{
    /// <summary>
    /// This class is used to create a window used by tortuga engine
    /// </summary>
    public class Window
    {
        /// <summary>
        /// Native SDL window class
        /// </summary>
        public NativeWindow NativeWindow => _nativeWindow;
        /// <summary>
        /// Swapchain used for vulkan renderer
        /// </summary>
        public Swapchain Swapchain => _swapchain;
        private NativeWindow _nativeWindow;
        private Swapchain _swapchain;

        /// <summary>
        /// Create a window
        /// </summary>
        /// <param name="title">title for the window</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        /// <param name="width">width of the window</param>
        /// <param name="height">height of the window</param>
        public Window(
            string title,
            int x, int y,
            int width, int height
        )
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            _nativeWindow = new NativeWindow(
                graphicsModule.GraphicsService,
                title,
                x, y,
                width, height,
                SDL_WindowFlags.Shown
            );
            _swapchain = new Swapchain(
                graphicsModule.GraphicsService.PrimaryDevice,
                _nativeWindow
            );
        }
    }
}