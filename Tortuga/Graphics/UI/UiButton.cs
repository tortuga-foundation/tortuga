using System;
using System.Drawing;
using System.Numerics;
using Tortuga.Input;
using Tortuga.Graphics.UI.Base;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a button element
    /// </summary>
    public class UiButton : UiInteractable
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

        /// <summary>
        /// Triggers when mouse enters the button
        /// </summary>
        public Action OnMouseEnter;
        /// <summary>
        /// Triggers when mouse leaves the button
        /// </summary>
        public Action OnMouseExit;
        /// <summary>
        /// Triggers when button is clicked
        /// </summary>
        public Action OnActive;

        /// <summary>
        /// Constructor for Ui button
        /// </summary>
        /// <param name="text">The string text that will appear inside the button, You can change this using setter</param>
        public UiButton(string text = "Button") : base()
        {
            _text = new UiText();
            _text.FontSize = 12.0f;
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
        }

        /// <summary>
        /// Get's called when mouse position changes
        /// </summary>
        /// <param name="position">New mouse position</param>
        protected override void OnMousePositionChanged(Vector2 position)
        {
            base.OnMousePositionChanged(position);
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

        /// <summary>
        /// Get's called when mouse button is released
        /// </summary>
        /// <param name="button">The identifier of the button that was released</param>
        protected override void OnMouseButtonUp(MouseButton button)
        {
            base.OnMouseButtonUp(button);
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

        /// <summary>
        /// Get's called when mouse button is pressed down
        /// </summary>
        /// <param name="button">The identifier of the button that was pressed</param>
        protected override void OnMouseButtonDown(MouseButton button)
        {
            base.OnMouseButtonDown(button);
            if (IsMouseInsideRect && button == MouseButton.Left)
                _state = ButtonStateType.Active;
        }

        /// <summary>
        /// Apply constraints to the position and scale with the background color for the button
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            try
            {
                if (_state == ButtonStateType.Normal)
                    Background = NormalBackground;
                else if (_state == ButtonStateType.Hover)
                    Background = HoverBackground;
                else if (_state == ButtonStateType.Active)
                    Background = ActiveBackground;
                else if (_state == ButtonStateType.Disabled)
                    Background = DisabledBackground;
            }
            catch (System.Exception) { }

            base.UpdatePositionsWithConstraints();
        }
    }
}