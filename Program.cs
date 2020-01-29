using System;

class Program
{
    unsafe static void Main(string[] args)
    {
        var instance = new Tortuga.Graphics.VulkanInstance();
        var window = new Tortuga.Graphics.Window(
            instance,
            "tortuga",
            0, 0,
            1920, 1080,
            Veldrid.Sdl2.SDL_WindowFlags.Resizable,
            true);
        while (window.NativeWindow.Exists)
        {
            window.PumpEvents();
        }
    }
}