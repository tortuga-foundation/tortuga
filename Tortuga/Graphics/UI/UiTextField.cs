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
        private bool _isLeftShiftDown;
        private bool _isRightShiftDown;

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
            InputSystem.OnKeyUp += OnKeyUp;
        }
        /// <summary>
        /// Deconstructor for Ui TextField
        /// </summary>
        ~UiTextField()
        {
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
            InputSystem.OnMouseButtonDown -= OnMouseButtonDown;
            InputSystem.OnKeyDown -= OnKeyDown;
            InputSystem.OnKeyUp += OnKeyUp;
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

        private void OnKeyDown(KeyCode key)
        {
            if (!IsFocused)
                return;

            switch (key)
            {
                case KeyCode.ShiftLeft:
                    _isLeftShiftDown = true;
                    break;
                case KeyCode.ShiftRight:
                    _isRightShiftDown = true;
                    break;
                case KeyCode.BackSpace:
                    if (this.Text.Length > 0)
                        this.Text = this.Text.Substring(0, this.Text.Length - 1);
                    break;
                case KeyCode.A:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "A";
                    else
                        this.Text += "a";
                    break;
                case KeyCode.B:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "B";
                    else
                        this.Text += "b";
                    break;
                case KeyCode.C:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "C";
                    else
                        this.Text += "c";
                    break;
                case KeyCode.D:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "D";
                    else
                        this.Text += "d";
                    break;
                case KeyCode.E:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "E";
                    else
                        this.Text += "e";
                    break;
                case KeyCode.F:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "F";
                    else
                        this.Text += "f";
                    break;
                case KeyCode.G:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "G";
                    else
                        this.Text += "g";
                    break;
                case KeyCode.H:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "H";
                    else
                        this.Text += "h";
                    break;
                case KeyCode.I:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "I";
                    else
                        this.Text += "i";
                    break;
                case KeyCode.J:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "J";
                    else
                        this.Text += "j";
                    break;
                case KeyCode.K:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "K";
                    else
                        this.Text += "k";
                    break;
                case KeyCode.L:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "L";
                    else
                        this.Text += "l";
                    break;
                case KeyCode.M:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "M";
                    else
                        this.Text += "m";
                    break;
                case KeyCode.N:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "N";
                    else
                        this.Text += "n";
                    break;
                case KeyCode.O:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "O";
                    else
                        this.Text += "o";
                    break;
                case KeyCode.P:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "P";
                    else
                        this.Text += "p";
                    break;
                case KeyCode.Q:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "Q";
                    else
                        this.Text += "q";
                    break;
                case KeyCode.R:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "R";
                    else
                        this.Text += "r";
                    break;
                case KeyCode.S:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "S";
                    else
                        this.Text += "s";
                    break;
                case KeyCode.T:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "T";
                    else
                        this.Text += "t";
                    break;
                case KeyCode.U:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "U";
                    else
                        this.Text += "u";
                    break;
                case KeyCode.V:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "V";
                    else
                        this.Text += "v";
                    break;
                case KeyCode.W:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "W";
                    else
                        this.Text += "w";
                    break;
                case KeyCode.X:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "X";
                    else
                        this.Text += "x";
                    break;
                case KeyCode.Y:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "Y";
                    else
                        this.Text += "y";
                    break;
                case KeyCode.Z:
                    if (_isLeftShiftDown || _isRightShiftDown)
                        this.Text += "Z";
                    else
                        this.Text += "z";
                    break;
            }
        }

        private void OnKeyUp(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.ShiftLeft:
                    _isLeftShiftDown = false;
                    break;
                case KeyCode.ShiftRight:
                    _isRightShiftDown = false;
                    break;
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