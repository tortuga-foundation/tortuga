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
        /// The element to scroll
        /// </summary>
        public UiElement Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                _viewport.Mask = _mask;
                this.Add(_viewport);
            }
        }
        private UiElement _viewport;
        /// <summary>
        /// Scroll amount for the Rect
        /// </summary>
        public Vector2 Scroll;

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

        /// <summary>
        /// contains current mouse position
        /// </summary>
        private Vector2 _mousePosition;

        /// <summary>
        /// Constructor for ui ui scroll rect
        /// </summary>
        public UiScrollRect()
        {
            _mask = new UiElement();
            _mask.PositionXConstraint = new PercentConstraint(0.0f);
            _mask.PositionYConstraint = new PercentConstraint(0.0f);
            _mask.ScaleXConstraint = new PercentConstraint(1.0f);
            _mask.ScaleYConstraint = new PercentConstraint(1.0f);
            this.Add(_mask);

            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnWheelDeltaChange += OnMouseWheelDeltaChanged;
        }
        /// <summary>
        /// De-constructor for ui ui scroll rect
        /// </summary>
        ~UiScrollRect()
        {
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
            InputSystem.OnWheelDeltaChange -= OnMouseWheelDeltaChanged;
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
            
            this.Viewport.PositionXConstraint = new PixelConstraint(this.Scroll.X);
            this.Viewport.PositionYConstraint = new PixelConstraint(this.Scroll.Y);
        }
    }
}