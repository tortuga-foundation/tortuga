using System;
using Tortuga.UI.Base;
using Tortuga.Input;

namespace Tortuga.UI
{
    /// <summary>
    /// Text field ui element
    /// </summary>
    public class UiTextField : UiInteractable
    {
        /// <summary>
        /// Different types of text field
        /// </summary>
        public enum TypeOfTextField
        {
            /// <summary>
            /// This is a normal text field which can have any character
            /// </summary>
            Normal,
            /// <summary>
            /// This is a float text field which can must have a float
            /// </summary>
            Float,
            /// <summary>
            /// This is a int text field which can must have a integer
            /// </summary>
            Int
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

        /// <summary>
        /// The type of text filed, for more information look at `TypeOfTextField`
        /// </summary>
        public TypeOfTextField Type
        {
            get => _type;
            set
            {
                ClearCursor();
                if (value == TypeOfTextField.Int)
                {
                    if (!Int32.TryParse(Text, out int result))
                        Text = "0";
                }
                else if (value == TypeOfTextField.Float)
                {
                    if (!Single.TryParse(Text, out float result))
                        Text = "0";
                }
                _type = value;
            }
        }
        private TypeOfTextField _type;

        /// <summary>
        /// Convert this text field to a float value
        /// </summary>
        /// <value></value>
        public float ValueFloat
        {
            get
            {
                if (Single.TryParse(Text, out float val))
                    return val;

                return 0.0f;
            }
        }
        /// <summary>
        /// Convert the text in this text field to an integer
        /// </summary>
        public int ValueInt
        {
            get
            {
                if (Int32.TryParse(Text, out int val))
                    return val;

                return 0;
            }
        }

        private UiText _text;
        private UiRenderable _block;
        private int _cursorPosition;
        private bool _displayingCursor;
        private float _timeElapsed;

        /// <summary>
        /// Constructor for ui text field
        /// </summary>
        public UiTextField(string value = "", string placeholder = "") : base()
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
        }

        /// <summary>
        /// Get's called when mouse button is pressed down
        /// </summary>
        /// <param name="button">The identifier of the button that was pressed</param>
        protected override void OnMouseButtonDown(MouseButton button)
        {
            base.OnMouseButtonDown(button);
            if (button == MouseButton.Left)
            {
                if (IsMouseInsideRect)
                {
                    if (!IsFocused)
                    {
                        IsFocused = true;
                        _cursorPosition = Text.Length;
                    }
                    else
                    {
                        ClearCursor();
                        var relativePosition = _mousePosition.X - AbsolutePosition.X - 5.0f;
                        float textPosition = 0.0f;
                        for (_cursorPosition = 0; _cursorPosition < Text.Length; _cursorPosition++)
                        {
                            var symbol = Array.Find(
                                _text.Font.Symbols,
                                (UiFont.Symbol symbol) => symbol.Identifier == Text[_cursorPosition]
                            );
                            if (symbol == null)
                                continue;
                            if (textPosition < relativePosition)
                                textPosition += symbol.AdvanceX * _text.FontSizeMultiplier;
                            else
                                break;
                        }
                    }
                }
                else
                {
                    IsFocused = false;
                    ClearCursor();
                }
            }
        }

        /// <summary>
        /// Get's called when a character was pressed on the keyboard
        /// </summary>
        /// <param name="character">the character that was pressed on the keyboard</param>
        protected override void OnCharacterPress(char character)
        {
            base.OnCharacterPress(character);
            if (!IsFocused)
                return;
            Text = this.Text.Insert(
                _cursorPosition,
                character.ToString()
            );
            _cursorPosition++;
        }

        /// <summary>
        /// Get's called when a keyboard key is pressed
        /// </summary>
        /// <param name="key">The identifier of the keyboard key that was pressed</param>
        /// <param name="modifiers">The modifiers being pressed with the key</param>
        protected override void OnKeyDown(KeyCode key, ModifierKeys modifiers)
        {
            base.OnKeyDown(key, modifiers);
            if (!IsFocused)
                return;

            if (key == KeyCode.BackSpace)
            {
                if (_cursorPosition > 0)
                {
                    ClearCursor();
                    _cursorPosition--;
                    this.Text = this.Text.Remove(_cursorPosition, 1);
                }
            }
            else if (key == KeyCode.Delete)
            {
                if (_cursorPosition < Text.Length - 1)
                {
                    ClearCursor();
                    this.Text = this.Text.Remove(_cursorPosition, 1);
                }
            }
            else if (key == KeyCode.Left)
            {
                if (_cursorPosition > 0)
                {
                    ClearCursor();
                    _cursorPosition--;
                }
            }
            else if (key == KeyCode.Right)
            {
                if (_cursorPosition < Text.Length - 1)
                {
                    ClearCursor();
                    _cursorPosition++;
                }
            }
        }

        /// <summary>
        /// update 
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            try
            {
                if (IsFocused)
                {
                    if (MathF.Round(_timeElapsed) / 40.0f > 1.0f)
                    {
                        if (_displayingCursor == false)
                        {
                            Text = Text.Insert(
                                _cursorPosition,
                                "|"
                            );
                            _displayingCursor = true;
                        }
                        else if (_displayingCursor)
                        {
                            this.Text = this.Text.Remove(_cursorPosition, 1);
                            _displayingCursor = false;
                        }
                        _timeElapsed = 0.0f;
                    }
                    else
                        _timeElapsed += Time.DeltaTime;
                }
            }
            catch (System.Exception) { }

            base.UpdatePositionsWithConstraints();
        }

        private void ClearCursor()
        {
            if (_displayingCursor)
            {
                this.Text = this.Text.Remove(_cursorPosition, 1);
                _displayingCursor = false;
                _timeElapsed = 0.0f;
            }
        }
    }
}