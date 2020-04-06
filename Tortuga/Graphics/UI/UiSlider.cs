using System.Drawing;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a slider ui element
    /// </summary>
    public class UiSlider : UiInteractable
    {
        /// <summary>
        /// Left value of the slider
        /// </summary>
        public float MinValue = 0.0f;
        /// <summary>
        /// Right value of the slider
        /// </summary>
        public float MaxValue = 1.0f;
        /// <summary>
        /// Current slider value
        /// </summary>
        public float Value
        {
            set
            {
                var offset = _thumb.Scale.X / Scale.X;
                if (value >= 1.0f - offset)
                    value = 1.0f - offset;
                else if (value < 0.0f)
                    value = 0.0f;
                _thumb.PositionXConstraint = new PercentConstraint(value);


                _value = value / (1.0f - offset);
                _value = MinValue + (_value * (MaxValue - MinValue));
            }
            get => _value;
        }
        /// <summary>
        /// Slider user interface element
        /// </summary>
        public UiRenderable Thumb => _thumb;
        /// <summary>
        /// Thumb user interface element
        /// </summary>
        public UiRenderable Slider => _slider;

        private float _value;
        private bool _isMouseButtonDown;
        private bool _isDraging;
        private UiRenderable _thumb;
        private UiRenderable _slider;

        /// <summary>
        /// Constructor for creating a ui slider element
        /// </summary>
        public UiSlider() : base()
        {
            _thumb = new UiRenderable();
            _thumb.Background = Color.FromArgb(255, 240, 68, 34);
            _thumb.PositionXConstraint = new PercentConstraint(0.0f);
            _thumb.PositionYConstraint = new PercentConstraint(0.0f);
            _thumb.ScaleXConstraint = new PixelConstraint(20.0f);
            _thumb.ScaleYConstraint = new PixelConstraint(20.0f);
            _thumb.BorderRadius = 10.0f;

            _slider = new UiRenderable();
            _slider.Background = Color.Gray;
            _slider.PositionXConstraint = new PixelConstraint(0.0f);
            _slider.PositionYConstraint = new PixelConstraint(5.0f);
            _slider.ScaleXConstraint = new PercentConstraint(1.0f);
            _slider.ScaleYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(10.0f);
            _slider.BorderRadius = 5.0f;

            // add slider and thumb as child ui elements
            this.Add(_slider);
            this.Add(_thumb);

            _isMouseButtonDown = false;
            _isDraging = false;
        }

        /// <summary>
        /// Get's called when mouse button is released
        /// </summary>
        /// <param name="button">The identifier of the button that was released</param>
        protected override void OnMouseButtonUp(MouseButton button)
        {
            base.OnMouseButtonUp(button);
            if (button == MouseButton.Left)
            {
                _isMouseButtonDown = false;
                _isDraging = false;
            }
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
                _isMouseButtonDown = true;
                if (IsMouseInsideRect)
                {
                    _isDraging = true;
                    UpdateSliderValueWithMousePosition();
                }
            }
        }

        /// <summary>
        /// Get's called when mouse position changes
        /// </summary>
        /// <param name="position">New mouse position</param>
        protected override void OnMousePositionChanged(Vector2 position)
        {
            base.OnMousePositionChanged(position);
            _mousePosition = position;
            if (_isDraging && _isMouseButtonDown)
                UpdateSliderValueWithMousePosition();
        }

        private void UpdateSliderValueWithMousePosition()
        {
            var absPos = AbsolutePosition;
            var val = ((_mousePosition.X - absPos.X - (_thumb.Scale.X / 2)) / Scale.X);
            Value = val;
        }
    }
}