using System;
using Vulkan;
using ImGuiNET;
using static ImGuiNET.ImGuiNative;

namespace Tortuga.Graphics
{
    public class ImGUI
    {
        private IntPtr _context;
        private ImGuiIOPtr _io;
        private API.Buffer _vertex;
        private API.Buffer _index;
        private API.Buffer _projection;

        public ImGUI()
        {
            //setup context
            _context = ImGui.CreateContext();
            ImGui.SetCurrentContext(_context);
            ImGui.StyleColorsDark();

            //setup io
            _io = ImGui.GetIO();
            _io.Fonts.AddFontDefault();

        }

        private void Render()
        {
            
        }
    }
}