using System;
using System.Numerics;
using Tortuga.Graphics.UI.Base;
using Tortuga.Input;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Can be used to create a scroll view
    /// </summary>
    public class UiScrollRect : UiElement
    {
        /// <summary>
        /// Can be used to hide, show or auto hide scroll bar
        /// </summary>
        public enum ScrollBarDisplayType
        {
            /// <summary>
            /// Auto hides the scroll bars if the content is small 
            /// or shows the scroll bar if the content is big  
            /// </summary>
            AutoHide,
            /// <summary>
            /// Always show the scroll bars
            /// </summary>
            AlwaysShow,
            /// <summary>
            /// Always hide the scroll bars
            /// </summary>
            AlwaysHide
        }

        /// <summary>
        /// The element to scroll
        /// </summary>
        public UiElement Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                _viewport.Mask = Mask;
                this.Add(_viewport);
            }
        }
        private UiElement _viewport;
        /// <summary>
        /// Scroll amount for the Rect
        /// </summary>
        public Vector2 Scroll;
        /// <summary>
        /// Vertical scroll bar display type
        /// </summary>
        public ScrollBarDisplayType VerticalScrollBarDisplayType;
        /// <summary>
        /// Horizontal scroll bar display type
        /// </summary>
        public ScrollBarDisplayType HorizontalScrollBarDisplayType;

        /// <summary>
        /// Returns true if mouse is inside the ui element
        /// </summary>
        public bool IsMouseInsideRect
        {
            get
            {
                var absPos = AbsolutePosition;
                return (
                    //make sure mouse is inside interactable component
                    _mousePosition.X >= absPos.X &&
                    _mousePosition.Y >= absPos.Y &&
                    _mousePosition.X <= absPos.X + Scale.X &&
                    _mousePosition.Y <= absPos.Y + Scale.Y
                );
            }
        }

        private Vector2 _mousePosition;
        private UiScrollBar _verticalScrollbar;
        private UiScrollBar _horizontalScrollbar;

        /// <summary>
        /// Constructor for ui ui scroll rect
        /// </summary>
        public UiScrollRect()
        {
            this.Mask = new UiElement();
            this.Mask.PositionXConstraint = new PercentConstraint(0.0f);
            this.Mask.PositionYConstraint = new PercentConstraint(0.0f);
            this.Mask.ScaleXConstraint = new PercentConstraint(1.0f);
            this.Mask.ScaleYConstraint = new PercentConstraint(1.0f);
            this.Add(this.Mask);

            _verticalScrollbar = new UiScrollBar();
            _verticalScrollbar.PositionXConstraint = new PercentConstraint(1.0f) - new PixelConstraint(5.0f);
            _verticalScrollbar.PositionYConstraint = new PercentConstraint(0.0f);
            _verticalScrollbar.ScaleXConstraint = new PixelConstraint(5.0f);
            _verticalScrollbar.ScaleYConstraint = new PercentConstraint(1.0f);
            _verticalScrollbar.Type = UiScrollBar.TypeOfSlider.Vertical;
            _verticalScrollbar.OnValueChanged += OnVerticalValueChanged;
            this.Add(_verticalScrollbar);

            _horizontalScrollbar = new UiScrollBar();
            _horizontalScrollbar.PositionXConstraint = new PercentConstraint(0.0f);
            _horizontalScrollbar.PositionYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(5.0f);
            _horizontalScrollbar.ScaleXConstraint = new PercentConstraint(1.0f);
            _horizontalScrollbar.ScaleYConstraint = new PixelConstraint(5.0f);
            _horizontalScrollbar.Type = UiScrollBar.TypeOfSlider.Horizontal;
            _horizontalScrollbar.OnValueChanged += OnHorizontalValueChanged;
            this.Add(_horizontalScrollbar);

            this.VerticalScrollBarDisplayType = ScrollBarDisplayType.AutoHide;
            this.HorizontalScrollBarDisplayType = ScrollBarDisplayType.AutoHide;

            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnWheelDeltaChange += OnMouseWheelDeltaChanged;
        }
        /// <summary>
        /// De-constructor for ui ui scroll rect
        /// </summary>
        ~UiScrollRect()
        {
            _verticalScrollbar.OnValueChanged -= OnVerticalValueChanged;
            _horizontalScrollbar.OnValueChanged -= OnHorizontalValueChanged;
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
            InputSystem.OnWheelDeltaChange -= OnMouseWheelDeltaChanged;
        }

        private void OnHorizontalValueChanged(float val)
        {
            Scroll.X = val * (Scale.X - Viewport.Scale.X);
            if (Single.IsNaN(Scroll.X))
                Scroll.X = 0.0f;
        }

        private void OnVerticalValueChanged(float val)
        {
            Scroll.Y = val * (Scale.Y - Viewport.Scale.Y);
            if (Single.IsNaN(Scroll.Y))
                Scroll.Y = 0.0f;
        }

        private void OnMouseWheelDeltaChanged(float wheel)
        {
            if (IsMouseInsideRect)
            {
                Scroll.Y += wheel * 10.0f;
                if (Scroll.Y > 0)
                    Scroll.Y = 0;
                else if (Scroll.Y < Scale.Y - Viewport.Scale.Y)
                {
                    if (Scale.Y < Viewport.Scale.Y)
                        Scroll.Y = Scale.Y - Viewport.Scale.Y;
                    else
                        Scroll.Y = 0.0f;
                }
                _verticalScrollbar.OnValueChanged -= OnVerticalValueChanged;
                _verticalScrollbar.Value = Scroll.Y / (Scale.Y - Viewport.Scale.Y);
                if (Single.IsNaN(Scroll.X))
                    Scroll.X = 0.0f;
                if (Single.IsNaN(Scroll.Y))
                    Scroll.Y = 0.0f;
                _verticalScrollbar.OnValueChanged += OnVerticalValueChanged;
            }
        }

        private void OnMousePositionChanged(Vector2 position)
        {
            _mousePosition = position;
        }

        /// <summary>
        /// Apply constraints to the position and scale
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            base.UpdatePositionsWithConstraints();
            if (Viewport == null)
                return;

            if (Viewport.Parent != this)
            {
                Console.WriteLine("Viewport must be a child of UiScrollRect");
                return;
            }
            
            //scroll viewport
            this.Viewport.PositionXConstraint = new PixelConstraint(this.Scroll.X);
            this.Viewport.PositionYConstraint = new PixelConstraint(this.Scroll.Y);

            bool shouldShowVerticalBar = (
                (
                    this.VerticalScrollBarDisplayType == ScrollBarDisplayType.AutoHide && 
                    this.Viewport.Scale.Y > this.Scale.Y
                ) ||
                this.VerticalScrollBarDisplayType == ScrollBarDisplayType.AlwaysShow
            );
            if (shouldShowVerticalBar)
            {
                this.Scale -= new Vector2(0.0f, 5.0f);
                _verticalScrollbar.IsEnabled = true;
            }
            else
            {
                _verticalScrollbar.IsEnabled = false;
            }
            
            bool shouldShowHorizontalBar = (
                (
                    this.HorizontalScrollBarDisplayType == ScrollBarDisplayType.AutoHide && 
                    this.Viewport.Scale.X > this.Scale.X
                ) ||
                this.HorizontalScrollBarDisplayType == ScrollBarDisplayType.AlwaysShow
            );
            if (shouldShowHorizontalBar)
            {
                this.Scale -= new Vector2(5.0f, 0.0f);
                _horizontalScrollbar.IsEnabled = true;
            }
            else
            {
                _horizontalScrollbar.IsEnabled = false;
            }
        }
    }
}