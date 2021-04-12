using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Main ui element
    /// </summary>
    public class UiElement : Core.BaseComponent
    {
        /// <summary>
        /// If false the ui will be skipped
        /// </summary>
        public bool IsEnabled;
        /// <summary>
        /// parent to this ui element
        /// </summary>
        public UiElement Parent => _parent;
        private UiElement _parent;
        /// <summary>
        /// All children of this ui element
        /// </summary>
        public UiElement[] Children => _children.ToArray();
        private List<UiElement> _children;

        /// <summary>
        /// Returns the absolute position of this ui element
        /// </summary>
        public Vector2 AbsolutePosition
        {
            get
            {
                var parentPosition = Vector2.Zero;
                if (_parent != null)
                    parentPosition = _parent.AbsolutePosition;
                if (_absolutePosition != this.Position + parentPosition)
                {
                    _absolutePosition = this.Position + parentPosition;
                    _isDirty = true;
                }
                return _absolutePosition;
            }
        }
        private Vector2 _absolutePosition;
        /// <summary>
        /// The position of this Ui Element
        /// </summary>
        public Vector2 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                    _isDirty = true;
                _position = value;
            }
        }
        private Vector2 _position;
        /// <summary>
        /// The Scale of this Ui Element
        /// </summary>
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                    _isDirty = true;
                _scale = value;
            }
        }
        private Vector2 _scale;
        /// <summary>
        /// The background color of this Ui element
        /// </summary>
        public Color Background
        {
            get => _background;
            set
            {
                if (_background != value)
                    _isDirty = true;
                _background = value;
            }
        }
        private Color _background = Color.White;

        /// <summary>
        /// Position X Constraints
        /// </summary>
        public Constraint PositionXConstraint;
        /// <summary>
        /// Position Y Constraints
        /// </summary>
        public Constraint PositionYConstraint;
        /// <summary>
        /// Scale X Constraints
        /// </summary>
        public Constraint ScaleXConstraint;
        /// <summary>
        /// Scale Y Constraints
        /// </summary>
        public Constraint ScaleYConstraint;

        /// <summary>
        /// If element is outside mask then it will not be rendered
        /// </summary>
        public UiElement Mask
        {
            get => _mask;
            set
            {
                foreach (var child in this.Children)
                    child.Mask = value;

                _mask = value;
            }
        }
        /// <summary>
        /// Where mask is stored
        /// </summary>
        protected UiElement _mask;

        /// <summary>
        /// Can be used to set border radius for all corners
        /// </summary>
        public float BorderRadius
        {
            set
            {
                this.BorderRadiusTopLeft = value;
                this.BorderRadiusTopRight = value;
                this.BorderRadiusBottomLeft = value;
                this.BorderRadiusBottomRight = value;
                _isDirty = true;
            }
        }
        /// <summary>
        /// Stores the border radius for top left corner
        /// </summary>
        public float BorderRadiusTopLeft
        {
            get => _borderRadiusTopLeft;
            set
            {
                if (_borderRadiusTopLeft != value)
                    _isDirty = true;
                _borderRadiusTopLeft = value;
            }
        }
        private float _borderRadiusTopLeft;
        /// <summary>
        /// stores the border radius of the top right corner
        /// </summary>
        public float BorderRadiusTopRight
        {
            get => _borderRadiusTopRight;
            set
            {
                if (_borderRadiusTopRight != value)
                    _isDirty = true;
                _borderRadiusTopRight = value;
            }
        }
        private float _borderRadiusTopRight;
        /// <summary>
        /// Stores the border radius of bottom left corner
        /// </summary>
        public float BorderRadiusBottomLeft
        {
            get => _borderRadiusBottomLeft;
            set
            {
                if (_borderRadiusBottomLeft != value)
                    _isDirty = true;
                _borderRadiusBottomLeft = value;
            }
        }
        private float _borderRadiusBottomLeft;
        /// <summary>
        /// stores the border radius of bottom right corner
        /// </summary>
        public float BorderRadiusBottomRight
        {
            get => _borderRadiusBottomRight;
            set
            {
                if (_borderRadiusBottomRight != value)
                    _isDirty = true;
                _borderRadiusBottomRight = value;
            }
        }
        private float _borderRadiusBottomRight;

        /// <summary>
        /// If set to true, then ui element will update gpu buffers
        /// </summary>
        protected bool _isDirty;

        /// <summary>
        /// Create a new ui element
        /// </summary>
        public UiElement()
        {
            this._children = new List<UiElement>();
            this.PositionXConstraint = null;
            this.PositionYConstraint = null;
            this.ScaleXConstraint = null;
            this.ScaleYConstraint = null;
            this.IsEnabled = true;
            _isDirty = true;
            _background = Color.White;
            _position = Vector2.Zero;
            _scale = new Vector2(100, 100);
        }

        /// <summary>
        /// Add a ui element as a child of this ui element
        /// </summary>
        /// <param name="element">element to add as a child</param>
        public virtual void Add(UiElement element)
        {
            if (_children.FindIndex(
                0, _children.Count,
                    (UiElement c) => c == element
                ) != -1
            )
                return;
            if (element._parent != null)
                element._parent.Remove(element);
            element._parent = this;
            if (this._mask != null)
                element.Mask = _mask;
            _children.Add(element);
            _isDirty = true;
        }
        /// <summary>
        /// Set a child element's parent to null
        /// </summary>
        /// <param name="element">child element to remove</param>
        public virtual void Remove(UiElement element)
        {
            if (
                _children.FindIndex(0, _children.Count,
                    (UiElement c) => c == element
                ) == -1
            )
                return;
            _children.Remove(element);
            element._parent = null;
            _isDirty = true;
        }

        /// <summary>
        /// Apply constraints to the position and scale
        /// </summary>
        public virtual void UpdatePositionsWithConstraints(Vector2 canvas)
        {
            var parentScale = canvas;

            if (_parent != null)
                parentScale = _parent.Scale;

            var position = this.Position;
            if (this.PositionXConstraint != null)
            {
                float pixel = 0.0f;
                var constraintValues = this.PositionXConstraint.Values;
                foreach (var val in constraintValues)
                {
                    if (val.Type == ConstraintType.Percent)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += (parentScale.X * val.Value);
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= (parentScale.X * val.Value);
                    }
                    else if (val.Type == ConstraintType.Pixel)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += val.Value;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= val.Value;
                    }
                }
                position.X = pixel;
            }

            if (this.PositionYConstraint != null)
            {
                float pixel = 0.0f;
                var constraintValues = this.PositionYConstraint.Values;
                foreach (var val in constraintValues)
                {
                    if (val.Type == ConstraintType.Percent)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += (parentScale.Y * val.Value);
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= (parentScale.Y * val.Value);
                    }
                    else if (val.Type == ConstraintType.Pixel)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += val.Value;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= val.Value;
                    }
                }
                position.Y = pixel;
            }
            this.Position = position;
            var scale = this.Scale;
            if (this.ScaleXConstraint != null)
            {
                float pixel = 0.0f;
                var constraintValues = this.ScaleXConstraint.Values;
                foreach (var val in constraintValues)
                {
                    if (val.Type == ConstraintType.Percent)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += (parentScale.X * val.Value);
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= (parentScale.X * val.Value);
                    }
                    else if (val.Type == ConstraintType.Pixel)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += val.Value;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= val.Value;
                    }
                    else if (val.Type == ConstraintType.ContentAutoFit)
                    {
                        float maximumWidth = 0.0f;
                        foreach (var child in Children)
                        {
                            if (child.Position.X + child.Scale.X > maximumWidth)
                                maximumWidth = child.Position.X + child.Scale.X;
                        }

                        if (val.Operator == ConstraintOperators.Add)
                            pixel += maximumWidth;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= maximumWidth;
                    }
                }
                scale.X = pixel;
            }

            if (this.ScaleYConstraint != null)
            {
                float pixel = 0.0f;
                var constraintValues = this.ScaleYConstraint.Values;
                foreach (var val in constraintValues)
                {
                    if (val.Type == ConstraintType.Percent)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += (parentScale.Y * val.Value);
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= (parentScale.Y * val.Value);
                    }
                    else if (val.Type == ConstraintType.Pixel)
                    {
                        if (val.Operator == ConstraintOperators.Add)
                            pixel += val.Value;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= val.Value;
                    }
                    else if (val.Type == ConstraintType.ContentAutoFit)
                    {
                        float maximumHeight = 0.0f;
                        foreach (var child in Children)
                        {
                            if (child.Position.Y + child.Scale.Y > maximumHeight)
                                maximumHeight = child.Position.Y + child.Scale.Y;
                        }

                        if (val.Operator == ConstraintOperators.Add)
                            pixel += maximumHeight;
                        else if (val.Operator == ConstraintOperators.Subtract)
                            pixel -= maximumHeight;
                    }
                }
                scale.Y = pixel;
            }
            this.Scale = scale;
        }

        /// <summary>
        /// Ask's for the ui element to be updated on next frame
        /// </summary>
        public void ReDraw()
        {
            _isDirty = true;
        }
    }
}