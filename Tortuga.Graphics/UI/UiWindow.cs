using System.Numerics;
using System.Drawing;
using Tortuga.Input;
using System;

namespace Tortuga.UI
{
    /// <summary>
    /// user interface window
    /// </summary>
    public class UiWindow : UiRenderable
    {
        /// <summary>
        /// contains the content for this ui window
        /// </summary>
        public UiElement Content => _content;
        private UiElement _content;
        /// <summary>
        /// The area of the window that allows the user to drag it arround
        /// </summary>
        public UiRenderable DraggableArea => _dragableArea;
        private UiRenderable _dragableArea;
        /// <summary>
        /// the title for this window
        /// </summary>
        public UiText Label => _label;
        private UiText _label;
        /// <summary>
        /// The area of the window that allows the user to resize it
        /// </summary>
        public UiRenderable ResizeableArea;
        private UiRenderable _resizeableArea;
        private bool _isDraggingWindow;
        private bool _isResizingWindow;

        /// <summary>
        /// constructor for user interface window
        /// </summary>
        public UiWindow()
        {
            _isDraggingWindow = false;
            _isResizingWindow = false;
            BorderRadius = 20;
            Scale = new Vector2(200, 500);
            Background = Color.Black;

            //setup dragable area
            _dragableArea = new UiRenderable();
            _dragableArea.BorderRadiusTopLeft = 20.0f;
            _dragableArea.BorderRadiusTopRight = 20.0f;
            _dragableArea.PositionXConstraint = new PercentConstraint(0.0f);
            _dragableArea.PositionYConstraint = new PercentConstraint(0.0f);
            _dragableArea.ScaleXConstraint = new PercentConstraint(1.0f);
            _dragableArea.ScaleYConstraint = new PixelConstraint(30.0f);
            _dragableArea.Background = Color.White;
            Add(_dragableArea);

            //setup window content
            _content = new UiElement();
            _content.PositionXConstraint = new PercentConstraint(0.0f);
            _content.PositionYConstraint = new PixelConstraint(30.0f);
            _content.ScaleXConstraint = new PercentConstraint(1.0f);
            _content.ScaleYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(30.0f);
            Add(_content);

            //setup window label
            _label = new UiText();
            _label.Text = "My Window";
            _label.HorizontalAlignment = UiHorizontalAlignment.Center;
            _label.VerticalAlignment = UiVerticalAlignment.Center;
            _label.Background = Color.Black;
            _label.PositionXConstraint = new PercentConstraint(0.0f);
            _label.PositionYConstraint = new PercentConstraint(0.0f);
            _label.ScaleXConstraint = new PercentConstraint(1.0f);
            _label.ScaleYConstraint = new PixelConstraint(30.0f);
            Add(_label);

            //setup resizeable area
            _resizeableArea = new UiRenderable();
            _resizeableArea.PositionXConstraint = new PercentConstraint(1.0f) - new PixelConstraint(20.0f);
            _resizeableArea.PositionYConstraint = new PercentConstraint(1.0f) - new PixelConstraint(20.0f);
            _resizeableArea.ScaleXConstraint = new PixelConstraint(20.0f);
            _resizeableArea.ScaleYConstraint = new PixelConstraint(20.0f);
            _resizeableArea.BorderRadiusBottomRight = 20.0f;
            _resizeableArea.Background = Color.White;
            Add(_resizeableArea);

            InputModule.OnMouseButtonDown += OnMouseButtonDown;
            InputModule.OnMouseButtonUp += OnMouseButtonUp;
            InputModule.OnMousePositionChanged += OnMousePositionChanged;
        }

        private void OnMouseButtonUp(MouseButton mouseButton)
        {
            _isDraggingWindow = false;
            _isResizingWindow = false;
        }

        private void OnMouseButtonDown(MouseButton mouseButton)
        {
            _isDraggingWindow = (
                mouseButton == MouseButton.Left &&
                _dragableArea.IsMouseInside
            );
            _isResizingWindow = (
                mouseButton == MouseButton.Left &&
                _resizeableArea.IsMouseInside
            );
        }

        private void OnMousePositionChanged(Vector2 mouseDelta)
        {
            if (_isDraggingWindow)
                Position += mouseDelta;
            if (_isResizingWindow)
                Scale += mouseDelta;
        }
    }
}