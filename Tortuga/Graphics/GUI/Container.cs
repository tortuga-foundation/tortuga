using System.Collections.Generic;
using System.Numerics;

namespace Tortuga.Graphics.GUI
{
    public enum TypeOfPosition
    {
        Relative,
        Fixed
    }

    public enum TypeOfDisplay
    {
        Normal,
        MaxWidth,
        Fill,
        Hide
    }

    public enum TypeOfOverFlow
    {
        Hide,
        Visible
    }

    public class Container
    {
        public TypeOfDisplay Display = TypeOfDisplay.Normal;
        public TypeOfPosition PositionType = TypeOfPosition.Relative;
        public TypeOfOverFlow OverflowX = TypeOfOverFlow.Hide;
        public TypeOfOverFlow OverflowY = TypeOfOverFlow.Hide;
        public Vector2 Position = Vector2.Zero;
        public Vector2 Scale = Vector2.One;
        public Vector4 BorderRadius = Vector4.Zero;
        public List<Container> Children => _children;
        private List<Container> _children;
        public Container Parent
        {
            set
            {
                if (value == this)
                    throw new System.NotSupportedException();

                _parent = value;
            }
            get => _parent;
        }
        private Container _parent;

        public Container()
        {
            this.Display = TypeOfDisplay.Normal;
            this.PositionType = TypeOfPosition.Relative;
            this.OverflowX = TypeOfOverFlow.Hide;
            this.OverflowY = TypeOfOverFlow.Hide;
            this.Position = Vector2.Zero;
            this.Scale = new Vector2(100, 100);
            this.BorderRadius = Vector4.Zero;
            this._children = new List<Container>();
            this._parent = null;
        }
    }
}