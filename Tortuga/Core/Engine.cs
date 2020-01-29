using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;
using Veldrid.Sdl2;
using System.Runtime.CompilerServices;

namespace Tortuga.Core
{
    public class Engine
    {
        private Sdl2Window _window;
        private GraphicsDevice _graphicsDevice;

        public Engine()
        {
            var windowOptions = new WindowCreateInfo
            {
                X = 50,
                Y = 50,
                WindowWidth = 1920,
                WindowHeight = 1080,
                WindowInitialState = Veldrid.WindowState.Normal,
                WindowTitle = "Tortuga"
            };
            var graphicsDeviceOptions = new GraphicsDeviceOptions
            {
                Debug = true,
                SwapchainDepthFormat = null,
                SyncToVerticalBlank = false,
                ResourceBindingModel = ResourceBindingModel.Improved,
                PreferDepthRangeZeroToOne = true,
                PreferStandardClipSpaceYDirection = true,
                SwapchainSrgbFormat = true
            };
            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowOptions,
                graphicsDeviceOptions,
                Settings.GraphicsAPI,
                out _window,
                out _graphicsDevice);
        }

        public void Run()
        {
            while(this._window.Exists)
            {
                Sdl2Events.ProcessEvents();
                _window.PumpEvents();
            }
        }
    }
}