using System.Drawing;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a slider ui element
    /// </summary>
    public class UiSlider : UiElement
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
        /// Slider user interface element
        /// </summary>
        public UiRenderable Thumb => _thumb;
        /// <summary>
        /// Thumb user interface element
        /// </summary>
        public UiRenderable Slider => _slider;

        private float _value;
        private Vector2 _mousePosition;
        private bool _isMouseButtonDown;
        private bool _isDraging;
        private UiRenderable _thumb;
        private UiRenderable _slider;

        /// <summary>
        /// Constructor for creating a ui slider element
        /// </summary>
        public UiSlider()
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
            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnMouseButtonDown += OnMouseDown;
            InputSystem.OnMouseButtonUp += OnMouseUp;
        }

        private void OnMouseUp(MouseButton mouseButtons)
        {
            if (mouseButtons == MouseButton.Left)
            {
                _isMouseButtonDown = false;
                _isDraging = false;
            }
        }

        private void OnMouseDown(MouseButton mouseButtons)
        {
            if (mouseButtons == MouseButton.Left)
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
        /// Deconstructor for Ui slider
        /// </summary>
        ~UiSlider()
        {
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
        }

        private void OnMousePositionChanged(Vector2 mousePosition)
        {
            _mousePosition = mousePosition;
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