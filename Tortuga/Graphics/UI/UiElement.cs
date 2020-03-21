using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Main ui element
    /// </summary>
    public class UiElement
    {
        /// <summary>
        /// parent to this ui element
        /// </summary>
        public UiElement Parent => _parent;
        /// <summary>
        /// All children of this ui element
        /// </summary>
        public UiElement[] Children => _children.ToArray();

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

                return this.Position + parentPosition;
            }
        }
        /// <summary>
        /// The position of this Ui Element
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The Scale of this Ui Element
        /// </summary>
        public Vector2 Scale;
        /// <summary>
        /// The background color of this Ui element
        /// </summary>
        public Color Background;

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

        private UiElement _parent;
        private List<UiElement> _children;

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
            if (Engine.Instance.CurrentScene != null)
                Engine.Instance.CurrentScene.UserInterface.Add(this);
        }

        /// <summary>
        /// Add a ui element as a child of this ui element
        /// </summary>
        /// <param name="element">element to add as a child</param>
        public void Add(UiElement element)
        {
            if (_children.FindIndex(
                0, _children.Count,
                    (UiElement c) => c == element
                ) != -1
            )
                return;
            element._parent = this;
            _children.Add(element);
            if (Engine.Instance.CurrentScene != null)
                Engine.Instance.CurrentScene.UserInterface.Remove(this);
        }
        /// <summary>
        /// Set a child element's parent to null
        /// </summary>
        /// <param name="element">child element to remove</param>
        public void Remove(UiElement element)
        {
            if (
                _children.FindIndex(0, _children.Count,
                    (UiElement c) => c == element
                ) == -1
            )
                return;
            _children.Remove(element);
            element._parent = null;
            if (Engine.Instance.CurrentScene != null)
                Engine.Instance.CurrentScene.UserInterface.Add(this);
        }

        /// <summary>
        /// Apply constraints to the position and scale
        /// </summary>
        public void UpdatePositionsWithConstraints()
        {
            var parentScale = new Vector2(
                Engine.Instance.MainWindow.Width,
                Engine.Instance.MainWindow.Height
            );

            if (_parent != null)
                parentScale = _parent.Scale;

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
                this.Position.X = pixel;
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
                this.Position.Y = pixel;
            }

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
                }
                this.Scale.X = pixel;
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
                }
                this.Scale.Y = pixel;
            }
        }
    }
}