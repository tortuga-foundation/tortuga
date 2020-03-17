using System.Collections.Generic;
using System.Numerics;

namespace Tortuga.Graphics.UI
{
    public class UiComponent
    {
        public static UiComponent Master = new UiComponent()
        {
            _parent = null,
            Constraints = new UiConstraints()
            {
                X = new Constraint
                {
                    Type = ConstraintType.Relative,
                    Value = 0
                },
                Y = new Constraint
                {
                    Type = ConstraintType.Relative,
                    Value = 0
                },
                Width = new Constraint
                {
                    Type = ConstraintType.Percent,
                    Value = 1
                },
                Height = new Constraint
                {
                    Type = ConstraintType.Percent,
                    Value = 1
                }
            }
        };

        public UiConstraints Constraints = new UiConstraints();
        public Vector4 Rect
        {
            get
            {
                var parentRect = new Vector4(
                    0, 0,
                    Engine.Instance.MainWindow.Width,
                    Engine.Instance.MainWindow.Height
                );
                if (this._parent != null)
                    parentRect = this._parent.Rect;

                var rect = Vector4.Zero;
                //width
                if (this.Constraints.Width.Type == ConstraintType.Absolute)
                    rect.Z = this.Constraints.Width.Value;
                else if (this.Constraints.Width.Type == ConstraintType.Relative)
                    rect.Z = this.Constraints.Width.Value;
                else if (this.Constraints.Width.Type == ConstraintType.Percent)
                    rect.Z = parentRect.Z * this.Constraints.Width.Value;
                else if (this.Constraints.Width.Type == ConstraintType.Max)
                    rect.Z = parentRect.Z - this.Constraints.Width.Value;

                //height
                if (this.Constraints.Height.Type == ConstraintType.Absolute)
                    rect.W = this.Constraints.Height.Value;
                else if (this.Constraints.Height.Type == ConstraintType.Relative)
                    rect.W = this.Constraints.Height.Value;
                else if (this.Constraints.Height.Type == ConstraintType.Percent)
                    rect.W = parentRect.Z * this.Constraints.Height.Value;
                else if (this.Constraints.Height.Type == ConstraintType.Max)
                    rect.W = parentRect.W - this.Constraints.Height.Value;

                //x
                if (this.Constraints.X.Type == ConstraintType.Absolute)
                    rect.X = this.Constraints.X.Value;
                else if (this.Constraints.X.Type == ConstraintType.Relative)
                    rect.X = parentRect.X + this.Constraints.X.Value;
                else if (this.Constraints.X.Type == ConstraintType.Percent)
                    rect.X = parentRect.X + (parentRect.Z * this.Constraints.X.Value);
                else if (this.Constraints.X.Type == ConstraintType.Max)
                    rect.X = parentRect.X + parentRect.Z - this.Constraints.X.Value;
            
                //y
                if (this.Constraints.Y.Type == ConstraintType.Absolute)
                    rect.Y = this.Constraints.Y.Value;
                else if (this.Constraints.Y.Type == ConstraintType.Relative)
                    rect.Y = parentRect.Y + this.Constraints.Y.Value;
                else if (this.Constraints.Y.Type == ConstraintType.Percent)
                    rect.Y = parentRect.Y + (parentRect.W * this.Constraints.Y.Value);
                else if (this.Constraints.Y.Type == ConstraintType.Max)
                    rect.Y = parentRect.Y + parentRect.W - this.Constraints.Y.Value;

                return rect;
            }
        }

        public List<UiComponent> AllChildren => _children;

        protected UiComponent _parent = null;
        protected List<UiComponent> _children = new List<UiComponent>();

        public void Add(UiComponent data)
        {
            data._parent = this;
            _children.Add(data);
        }

        public void Remove(UiComponent data)
        {
            _children.Remove(data);
        }
    }
}