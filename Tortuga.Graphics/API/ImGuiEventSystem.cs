using System;
using System.Numerics;
using ImGuiNET;
using Tortuga.Input;

namespace Tortuga.Graphics.API
{
    internal class UserInterfaceEventSystem
    {

        private unsafe void SetupKeyMappings()
        {
            var io = ImGui.GetIO();
            var keyNames = Enum.GetNames(typeof(ImGuiKey));
            foreach (var keyName in keyNames)
            {
                if (Enum.TryParse<ImGuiKey>(keyName, false, out ImGuiKey imGuiKey) == false)
                    continue;
                if (Enum.TryParse<KeyCode>(keyName, true, out KeyCode keyCode) == false)
                    continue;

                io.KeyMap[(int)imGuiKey] = (int)keyCode;
            }
        }

        public UserInterfaceEventSystem()
        {
            this.SetupKeyMappings();

            InputModule.OnMousePositionChanged += OnMousePositionChanged;
            InputModule.OnMouseButtonDown += OnMouseButtonDown;
            InputModule.OnMouseButtonUp += OnMouseButtonUp;
            InputModule.OnMouseWheelChange += OnMouseWheelChanged;
            InputModule.OnKeyDown += OnKeyDown;
            InputModule.OnKeyUp += OnKeyUp;
            InputModule.OnTextInput += OnTextInput;
        }

        private void OnTextInput(char character)
        {
            var io = ImGui.GetIO();
            io.AddInputCharacter(character);
        }

        private void OnMouseButtonDown(MouseButton button)
        {
            var io = ImGui.GetIO();
            if (button == MouseButton.Left)
                io.MouseDown[0] = true;
            else if (button == MouseButton.Middle)
                io.MouseDown[1] = true;
            else if (button == MouseButton.Right)
                io.MouseDown[2] = true;
        }

        private void OnMouseButtonUp(MouseButton button)
        {
            var io = ImGui.GetIO();
            if (button == MouseButton.Left)
                io.MouseDown[0] = false;
            else if (button == MouseButton.Middle)
                io.MouseDown[1] = false;
            else if (button == MouseButton.Right)
                io.MouseDown[2] = false;
        }

        private void OnMouseWheelChanged(Vector2 delta)
        {
            var io = ImGui.GetIO();
            io.MouseWheel = delta.Y;
        }

        private void OnKeyUp(KeyCode key, ModifierKeys modifiers)
        {
            var io = ImGui.GetIO();
            io.KeysDown[(int)key] = false;

            io.KeyCtrl = (modifiers & ModifierKeys.LeftControl) != 0 || (modifiers & ModifierKeys.RightControl) != 0;
            io.KeyAlt = (modifiers & ModifierKeys.LeftAlt) != 0 || (modifiers & ModifierKeys.RightAlt) != 0;
            io.KeyShift = (modifiers & ModifierKeys.LeftShift) != 0 || (modifiers & ModifierKeys.RightShift) != 0;
            io.KeySuper = (modifiers & ModifierKeys.LeftGui) != 0 || (modifiers & ModifierKeys.RightGui) != 0;
        }

        private void OnKeyDown(KeyCode key, ModifierKeys modifiers)
        {
            var io = ImGui.GetIO();
            io.KeysDown[(int)key] = true;

            io.KeyCtrl = (modifiers & ModifierKeys.LeftControl) != 0 || (modifiers & ModifierKeys.RightControl) != 0;
            io.KeyAlt = (modifiers & ModifierKeys.LeftAlt) != 0 || (modifiers & ModifierKeys.RightAlt) != 0;
            io.KeyShift = (modifiers & ModifierKeys.LeftShift) != 0 || (modifiers & ModifierKeys.RightShift) != 0;
            io.KeySuper = (modifiers & ModifierKeys.LeftGui) != 0 || (modifiers & ModifierKeys.RightGui) != 0;
        }

        private void OnMousePositionChanged(Vector2 mouseDelta)
        {
            var io = ImGui.GetIO();
            io.MousePos = InputModule.MousePosition;
        }
    }
}