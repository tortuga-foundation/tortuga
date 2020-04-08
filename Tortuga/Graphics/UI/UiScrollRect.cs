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
        public UiElement Viewport;
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
                Scroll.Y += wheel;
                if (Scroll.Y > 0)
                    Scroll.Y = 0;
                else if (Scroll.Y < Scale.Y - Viewport.Scale.Y)
                    Scroll.Y = Scale.Y - Viewport.Scale.Y;
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