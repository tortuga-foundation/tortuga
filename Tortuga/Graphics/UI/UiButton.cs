using System;
using System.Drawing;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a button element
    /// </summary>
    public class UiButton : UiRenderable
    {
        /// <summary>
        /// Type of button states
        /// </summary>
        public enum ButtonStateType
        {
            /// <summary>
            /// Normal button state, when it is idle
            /// </summary>
            Normal,
            /// <summary>
            /// button state when the mouse is hovering over the button 
            /// </summary>
            Hover,
            /// <summary>
            /// Normal button state, when it is idle
            /// </summary>
            Active,
            /// <summary>
            /// Normal button state, when it is idle
            /// </summary>
            Disabled
        }

        /// <summary>
        /// The color of the background when the button is idle
        /// </summary>
        public Color NormalBackground;
        /// <summary>
        /// The color of the the background when mouse is hovering on it
        /// </summary>
        public Color HoverBackground;
        /// <summary>
        /// The color of the the background on mouse click
        /// </summary>
        public Color ActiveBackground;
        /// <summary>
        /// The color of the the background when mouse the button is disabled
        /// </summary>
        public Color DisabledBackground;

        /// <summary>
        /// The text component showed inside the button
        /// </summary>
        public UiText Text => _text;
        private UiText _text;

        /// <summary>
        /// The current state of the button
        /// </summary>
        public ButtonStateType State => _state;
        private ButtonStateType _state;

        private Action OnMouseEnter;
        private Action OnMouseExit;
        private Action OnActive;

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

        private Vector2 _mousePosition = Vector2.Zero;

        /// <summary>
        /// Constructor for Ui button
        /// </summary>
        /// <param name="OnClick">What happens if the button is clicked</param>
        public UiButton(string text = "Button")
        {
            _text = new UiText();
            _text.FontSize = 14.0f;
            _text.PositionXConstraint = new PixelConstraint(0.0f);
            _text.PositionYConstraint = new PixelConstraint(0.0f);
            _text.ScaleXConstraint = new PercentConstraint(1.0f);
            _text.ScaleYConstraint = new PercentConstraint(1.0f);
            _text.Text = text;
            _text.HorizontalAlignment = UiHorizontalAlignment.Center;
            _text.VerticalAlignment = UiVerticalAlignment.Center;
            this.Add(_text);

            //default settings
            this.BorderRadius = 10;
            this.PositionXConstraint = new Graphics.UI.PixelConstraint(0.0f);
            this.PositionYConstraint = new Graphics.UI.PixelConstraint(0.0f);
            this.ScaleXConstraint = new Graphics.UI.PercentConstraint(1.0f);
            this.ScaleYConstraint = new Graphics.UI.PixelConstraint(40);
            this.NormalBackground = Color.LightSlateGray;
            this.HoverBackground = Color.PaleVioletRed;
            this.ActiveBackground = Color.DarkRed;

            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnMouseButtonDown += OnMouseButtonDown;
            InputSystem.OnMouseButtonUp += OnMouseButtonUp;
        }

        /// <summary>
        /// De-Constructor for ui button
        /// </summary> 
        ~UiButton()
        {
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
        }

        private void OnMousePositionChanged(Vector2 mousePosition)
        {
            _mousePosition = mousePosition;
            if (IsMouseInsideRect && _state == ButtonStateType.Normal)
            {
                OnMouseEnter?.Invoke();
                _state = ButtonStateType.Hover;
            }
            else if (!IsMouseInsideRect && _state != ButtonStateType.Normal)
            {
                OnMouseExit?.Invoke();
                _state = ButtonStateType.Normal;
            }
        }

        private void OnMouseButtonUp(MouseButton button)
        {
            if (button == MouseButton.Left && _state == ButtonStateType.Active)
            {
                if (IsMouseInsideRect)
                {
                    OnActive?.Invoke();
                    _state = ButtonStateType.Hover;
                }
                else
                    _state = ButtonStateType.Normal;
            }
        }

        private void OnMouseButtonDown(MouseButton button)
        {
            if (IsMouseInsideRect && button == MouseButton.Left)
                _state = ButtonStateType.Active;
        }

        private Color SmoothTransition(Color oldColor, Color newColor, float amount)
        {
            float r = oldColor.R;
            float g = oldColor.G;
            float b = oldColor.B;
            float a = oldColor.A;

            if (r != newColor.R)
                r += amount * (newColor.R - r);

            if (g != newColor.G)
                g += amount * (newColor.G - g);

            if (b != newColor.B)
                b += amount * (newColor.B - b);

            if (a != newColor.A)
                a += amount * (newColor.A - a);

            //safe color clamping
            {
                if (r > 255)
                    r = 255;
                else if (r < 0)
                    r = 0;

                if (g > 255)
                    g = 255;
                else if (g < 0)
                    g = 0;

                if (b > 255)
                    b = 255;
                else if (b < 0)
                    b = 0;

                if (a > 255)
                    a = 255;
                else if (a < 0)
                    a = 0;
            }

            return Color.FromArgb(
                Convert.ToInt32(MathF.Round(a)),
                Convert.ToInt32(MathF.Round(r)),
                Convert.ToInt32(MathF.Round(g)),
                Convert.ToInt32(MathF.Round(b))
            );
        }

        internal override API.BufferTransferObject[] UpdateBuffer()
        {
            if (_state == ButtonStateType.Normal)
                Background = SmoothTransition(Background, NormalBackground, Time.DeltaTime * 0.1f);
            else if (_state == ButtonStateType.Hover)
                Background = SmoothTransition(Background, HoverBackground, Time.DeltaTime * 0.1f);
            else if (_state == ButtonStateType.Active)
                Background = SmoothTransition(Background, ActiveBackground, Time.DeltaTime * 0.1f);
            else if (_state == ButtonStateType.Disabled)
                Background = SmoothTransition(Background, DisabledBackground, Time.DeltaTime * 0.1f);

            return base.UpdateBuffer();
        }
    }
}