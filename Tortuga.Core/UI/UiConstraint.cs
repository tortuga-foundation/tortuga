using System.Collections.Generic;

namespace Tortuga.UI
{
    /// <summary>
    /// Constraint Types
    /// </summary>
    public enum ConstraintType
    {
        /// <summary>
        /// Pixel Constraint
        /// </summary>
        Pixel,
        /// <summary>
        /// Percent Constraint
        /// </summary>
        Percent,
        /// <summary>
        /// Auto fit content
        /// </summary>
        ContentAutoFit
    }

    /// <summary>
    /// Constraint Operator Types
    /// </summary>
    public enum ConstraintOperators
    {
        /// <summary>
        /// Add
        /// </summary>
        Add,
        /// <summary>
        /// Subtract
        /// </summary>
        Subtract
    }

    /// <summary>
    /// Base constraint object, please use pixel constraint or percent constraint
    /// </summary>
    public class Constraint
    {
        /// <summary>
        /// Used to store constraint values
        /// </summary>
        public struct ConstraintValue
        {
            /// <summary>
            /// Type of constraint
            /// </summary>
            public ConstraintType Type;
            /// <summary>
            /// The constraint value
            /// </summary>
            public float Value;
            /// <summary>
            /// The operator of this constraint
            /// </summary>
            public ConstraintOperators Operator;
        }

        /// <summary>
        /// total constraint value
        /// </summary>
        public ConstraintValue[] Values => _values.ToArray();

        /// <summary>
        /// total constraint value
        /// </summary>
        protected List<ConstraintValue> _values = new List<ConstraintValue>();

        /// <summary>
        /// Add constraints
        /// </summary>
        /// <param name="a">left constraint</param>
        /// <param name="b">right constraint</param>
        /// <returns>added constraint</returns>
        public static Constraint operator +(Constraint a, Constraint b)
        {
            foreach (var v in b._values)
            {
                a._values.Add(new ConstraintValue
                {
                    Operator = ConstraintOperators.Add,
                    Type = v.Type,
                    Value = v.Value
                });
            }
            return a;
        }
        /// <summary>
        /// Subtract constrains
        /// </summary>
        /// <param name="a">left constraint</param>
        /// <param name="b">right constraint</param>
        /// <returns>subtracted constraint</returns>
        public static Constraint operator -(Constraint a, Constraint b)
        {
            foreach (var v in b._values)
            {
                a._values.Add(new ConstraintValue
                {
                    Operator = ConstraintOperators.Subtract,
                    Type = v.Type,
                    Value = v.Value
                });
            }
            return a;
        }
    }

    /// <summary>
    /// Uses pixels to constraint the ui element
    /// </summary>
    public class PixelConstraint : Constraint
    {
        /// <summary>
        /// constructor for pixel constraint
        /// </summary>
        /// <param name="value">pixel value</param>
        public PixelConstraint(float value)
        {
            this._values.Add(new ConstraintValue
            {
                Operator = ConstraintOperators.Add,
                Type = ConstraintType.Pixel,
                Value = value
            });
        }
    }

    /// <summary>
    /// Uses percent to constraint the ui element
    /// </summary>
    public class PercentConstraint : Constraint
    {
        /// <summary>
        /// constructor for percent constraint
        /// </summary>
        /// <param name="value">percent value between 0.0 and 1.0</param>
        public PercentConstraint(float value)
        {
            this._values.Add(new ConstraintValue
            {
                Operator = ConstraintOperators.Add,
                Type = ConstraintType.Percent,
                Value = value
            });
        }
    }

    /// <summary>
    /// Used to auto resize the width or height of a Ui element
    /// </summary>
    public class ContentAutoFitConstraint: Constraint
    {
        /// <summary>
        /// constructor for content auto fit constraint
        /// </summary>
        public ContentAutoFitConstraint()
        {
            this._values.Add(new ConstraintValue
            {
                Operator = ConstraintOperators.Add,
                Type = ConstraintType.ContentAutoFit,
                Value = 0
            });
        }
    }
}