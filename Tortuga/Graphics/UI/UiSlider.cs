using System.Drawing;
using System.Numerics;
using Tortuga.Input;
using Tortuga.Graphics.UI.Base;
using System;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Creates a slider ui element
    /// </summary>
    public class UiSlider : UiInteractable
    {
        /// <summary>
        /// Get's called when value is changed
        /// </summary>
        public Action<float> OnValueChanged;

        /// <summary>
        /// Different types of sliders
        /// </summary>
        public enum TypeOfSlider
        {
            /// <summary>
            /// Set the slider type to horizontal
            /// </summary>
            Horizontal,
            /// <summary>
            /// Set the slider type to Vertial
            /// </summary>
            Vertical
        }

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
                var offset = _type == TypeOfSlider.Horizontal ? 
                    _thumb.Scale.X / Scale.X : 
                    _thumb.Scale.Y / Scale.Y;

                if (value >= 1.0f - offset)
                    value = 1.0f - offset;
                else if (value < 0.0f)
                    value = 0.0f;

                if (_type == TypeOfSlider.Horizontal)
                    _thumb.PositionXConstraint = new PercentConstraint(value);
                else if (_type == TypeOfSlider.Vertical)
                    _thumb.PositionYConstraint = new PercentConstraint(value);

                _value = value / (1.0f - offset);
                _value = MinValue + (_value * (MaxValue - MinValue));
                OnValueChanged?.Invoke(_value);
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
        /// <summary>
        /// The type of slider, Look at 'TypeOfSlider'
        /// </summary>
        public TypeOfSlider Type
        {
            get => _type;
            set
            {
                ChangeSliderType(value);
                _type = value;
                _isDirty = true;
            }
        }
        private TypeOfSlider _type;

        private float _value;
        private bool _isMouseButtonDown;
        private bool _isDraging;
        /// <summary>
        /// The thumb of the slider
        /// </summary>
        protected UiRenderable _thumb;
        /// <summary>
        /// background of the slider
        /// </summary>
        protected UiRenderable _slider;

        /// <summary>
        /// Constructor for creating a ui slider element
        /// </summary>
        public UiSlider() : base()
        {
            this.Background = Color.Transparent;
            this.ScaleXConstraint = new PercentConstraint(1.0f);
            this.ScaleYConstraint = new PixelConstraint(20.0f);

            _thumb = new UiRenderable();
            _thumb.Background = Color.FromArgb(255, 240, 68, 34);
            _slider = new UiRenderable();
            _slider.Background = Color.Gray;

            // add slider and thumb as child ui elements
            this.Add(_slider);
            this.Add(_thumb);

            ChangeSliderType(_type);

            _isMouseButtonDown = false;
            _isDraging = false;
            _type = TypeOfSlider.Horizontal;
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
            if (_isDraging && _isMouseButtonDown)
                UpdateSliderValueWithMousePosition();
        }

        private void UpdateSliderValueWithMousePosition()
        {
            var absPos = AbsolutePosition;
            float val = 0.0f;
            if (_type == TypeOfSlider.Horizontal)
                val = ((_mousePosition.X - absPos.X - (_thumb.Scale.X / 2)) / Scale.X);
            else if (_type == TypeOfSlider.Vertical)
                val = ((_mousePosition.Y - absPos.Y - (_thumb.Scale.Y / 2)) / Scale.Y);
            Value = val;
        }

        /// <summary>
        /// Set's up the slider to be used in vertial or horizontal mode
        /// </summary>
        /// <param name="type">Vertial or Horizontal</param>
        protected virtual void ChangeSliderType(TypeOfSlider type)
        {
            if (type == TypeOfSlider.Horizontal)
            {
                this.ScaleXConstraint = new PercentConstraint(1.0f);
                this.ScaleYConstraint = new PixelConstraint(20.0f);

                _thumb.PositionXConstraint = new PercentConstraint(0.0f);
                _thumb.PositionYConstraint = new PercentConstraint(0.0f);
                _thumb.ScaleXConstraint = new PixelConstraint(20.0f);
                _thumb.ScaleYConstraint = new PixelConstraint(20.0f);
                _thumb.BorderRadius = 10.0f;

                _slider.PositionXConstraint = new PixelConstraint(0.0f);
                _slider.PositionYConstraint = new PixelConstraint(5.0f);
                _slider.ScaleXConstraint = new PercentConstraint(1.0f);
                _slider.ScaleYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(10.0f);
                _slider.BorderRadius = 5.0f;
            }
            else if (type == TypeOfSlider.Vertical)
            {
                this.ScaleXConstraint = new PixelConstraint(20.0f);
                this.ScaleYConstraint = new PercentConstraint(1.0f);

                _thumb.PositionXConstraint = new PercentConstraint(0.0f);
                _thumb.PositionYConstraint = new PercentConstraint(0.0f);
                _thumb.ScaleXConstraint = new PixelConstraint(20.0f);
                _thumb.ScaleYConstraint = new PixelConstraint(20.0f);
                _thumb.BorderRadius = 10.0f;

                _slider.PositionXConstraint = new PixelConstraint(5.0f);
                _slider.PositionYConstraint = new PixelConstraint(0.0f);
                _slider.ScaleXConstraint = new PercentConstraint(1.0f) - new PixelConstraint(10.0f);
                _slider.ScaleYConstraint = new PercentConstraint(1.0f);
                _slider.BorderRadius = 5.0f;
            }
        }
    }
}