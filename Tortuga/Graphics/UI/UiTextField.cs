using System;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Text field ui element
    /// </summary>
    public class UiTextField : UiElement
    {
        /// <summary>
        /// Returns true if mouse is inside the ui element
        /// </summary>
        public bool IsMouseInsideRect
        {
            get
            {
                var absPos = AbsolutePosition;

                return (
                    _mousePosition.X >= absPos.X &&
                    _mousePosition.Y >= absPos.Y &&
                    _mousePosition.X <= absPos.X + Scale.X &&
                    _mousePosition.Y <= absPos.Y + Scale.Y
                );
            }
        }

        /// <summary>
        /// If set to true than any key pressed will update the text field
        /// </summary>
        public bool IsFocused = false;

        /// <summary>
        /// Text value inside the text field
        /// </summary>
        public string Text
        {
            get => _text.Text;
            set => _text.Text = value;
        }

        private UiText _text;
        private UiRenderable _block;
        private Vector2 _mousePosition;

        /// <summary>
        /// Constructor for ui text field
        /// </summary>
        public UiTextField(string value = "", string placeholder = "")
        {
            _isDirty = true;

            _block = new UiRenderable();
            _block.BorderRadius = 5;
            _block.PositionXConstraint = new PixelConstraint(0.0f);
            _block.PositionYConstraint = new PixelConstraint(0.0f);
            _block.ScaleXConstraint = new PercentConstraint(1.0f);
            _block.ScaleYConstraint = new PercentConstraint(1.0f);

            _text = new UiText();
            _text.Text = "Test";
            _text.FontSize = 8;
            _text.PositionXConstraint = new PixelConstraint(5.0f);
            _text.PositionYConstraint = new PixelConstraint(5.0f);
            _text.ScaleXConstraint = new PercentConstraint(1.0f) - new PixelConstraint(10.0f);
            _text.ScaleYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(10.0f);

            this.Add(_block);
            this.Add(_text);

            this.ScaleXConstraint = new PercentConstraint(1.0f);
            this.ScaleYConstraint = new PixelConstraint(30.0f);

            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnMouseButtonDown += OnMouseButtonDown;
            InputSystem.OnKeyDown += OnKeyDown;
            InputSystem.OnCharacterPress += OnCharPress;
        }
        /// <summary>
        /// Deconstructor for Ui TextField
        /// </summary>
        ~UiTextField()
        {
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
            InputSystem.OnMouseButtonDown -= OnMouseButtonDown;
            InputSystem.OnKeyDown -= OnKeyDown;
            InputSystem.OnCharacterPress -= OnCharPress;
        }

        private void OnMousePositionChanged(Vector2 position)
        {
            _mousePosition = position;
        }

        private void OnMouseButtonDown(MouseButton button)
        {
            if (button == MouseButton.Left)
                IsFocused = IsMouseInsideRect;
        }

        private void OnCharPress(char character)
        {
            if (!IsFocused)
                return;
            Text += character;
        }

        private void OnKeyDown(KeyCode key)
        {
            if (!IsFocused)
                return;

            if (key == KeyCode.BackSpace)
            {
                if (this.Text.Length > 0)
                    this.Text = this.Text.Substring(0, this.Text.Length - 1);
            }
        }

        /// <summary>
        /// update 
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            base.UpdatePositionsWithConstraints();
        }
    }
}