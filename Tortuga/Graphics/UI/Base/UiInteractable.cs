using System;
using System.Numerics;
using Tortuga.Input;

namespace Tortuga.Graphics.UI.Base
{
    /// <summary>
    /// Base class for creating a interactable user interface element
    /// </summary>
    public class UiInteractable : UiRenderable
    {

        /// <summary>
        /// Returns true if mouse is inside the ui element
        /// </summary>
        public bool IsMouseInsideRect
        {
            get
            {
                var absPos = AbsolutePosition;
                if (this.Mask == null)
                {
                    return (
                        //make sure mouse is inside interactable component
                        _mousePosition.X >= absPos.X &&
                        _mousePosition.Y >= absPos.Y &&
                        _mousePosition.X <= absPos.X + Scale.X &&
                        _mousePosition.Y <= absPos.Y + Scale.Y
                    );
                }
                
                var mask = this.Mask.AbsolutePosition;
                return (
                    //make sure mouse is inside interactable component
                    _mousePosition.X >= absPos.X &&
                    _mousePosition.Y >= absPos.Y &&
                    _mousePosition.X <= absPos.X + Scale.X &&
                    _mousePosition.Y <= absPos.Y + Scale.Y &&
                    //make sure mouse is inside mask
                    _mousePosition.X >= mask.X &&
                    _mousePosition.Y >= mask.Y &&
                    _mousePosition.X <= mask.X + this.Mask.Scale.X &&
                    _mousePosition.Y <= mask.Y + this.Mask.Scale.Y
                );
            }
        }

        /// <summary>
        /// contains current mouse position
        /// </summary>
        protected Vector2 _mousePosition;

        /// <summary>
        /// Constructor for ui interactable
        /// </summary>
        public UiInteractable()
        {
            InputSystem.OnCharacterPress += OnCharacterPress;
            InputSystem.OnKeyDown += OnKeyDown;
            InputSystem.OnKeyUp += OnKeyUp;
            InputSystem.OnMousePositionChanged += OnMousePositionChanged;
            InputSystem.OnMouseButtonDown += OnMouseButtonDown;
            InputSystem.OnMouseButtonUp += OnMouseButtonUp;
        }
        /// <summary>
        /// De-constructor for ui interactable
        /// </summary>
        ~UiInteractable()
        {
            InputSystem.OnCharacterPress -= OnCharacterPress;
            InputSystem.OnKeyDown -= OnKeyDown;
            InputSystem.OnKeyUp -= OnKeyUp;
            InputSystem.OnMousePositionChanged -= OnMousePositionChanged;
            InputSystem.OnMouseButtonDown -= OnMouseButtonDown;
            InputSystem.OnMouseButtonUp -= OnMouseButtonUp;
        }

        /// <summary>
        /// Get's called when mouse button is released
        /// </summary>
        /// <param name="button">The identifier of the button that was released</param>
        protected virtual void OnMouseButtonUp(MouseButton button)
        {
        }

        /// <summary>
        /// Get's called when mouse button is pressed down
        /// </summary>
        /// <param name="button">The identifier of the button that was pressed</param>
        protected virtual void OnMouseButtonDown(MouseButton button)
        {
        }

        /// <summary>
        /// Get's called when mouse position changes
        /// </summary>
        /// <param name="position">New mouse position</param>
        protected virtual void OnMousePositionChanged(Vector2 position)
        {
            _mousePosition = position;
        }

        /// <summary>
        /// Get's called when a keyboard key is released
        /// </summary>
        /// <param name="key">The identifier of the keyboard key that was released</param>
        protected virtual void OnKeyUp(KeyCode key)
        {
        }


        /// <summary>
        /// Get's called when a keyboard key is pressed
        /// </summary>
        /// <param name="key">The identifier of the keyboard key that was pressed</param>
        protected virtual void OnKeyDown(KeyCode key)
        {
        }

        /// <summary>
        /// Get's called when a character was pressed on the keyboard
        /// </summary>
        /// <param name="character">the character that was pressed on the keyboard</param>
        protected virtual void OnCharacterPress(char character)
        {
        }
    }
}