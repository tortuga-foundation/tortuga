using System;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a button element
    /// </summary>
    public class UiButton : UiRenderable
    {
        /// <summary>
        /// The text component showed inside the button
        /// </summary>
        public UiText Text => _text;
        private UiText _text;

        /// <summary>
        /// Constructor for Ui button
        /// </summary>
        /// <param name="OnClick">What happens if the button is clicked</param>
        public UiButton(Action OnClick = null)
        {
            _text = new UiText();
            _text.FontSize = 10.0f;
            _text.PositionXConstraint = new PixelConstraint(0.0f);
            _text.PositionYConstraint = new PixelConstraint(0.0f);
            _text.ScaleXConstraint = new PercentConstraint(1.0f);
            _text.ScaleYConstraint = new PercentConstraint(1.0f);
            _text.Text = "Button";
            _text.HorizontalAlignment = UiHorizontalAlignment.Center;
            _text.VerticalAlignment = UiVerticalAlignment.Center;
            this.Add(_text);
        }
    }
}