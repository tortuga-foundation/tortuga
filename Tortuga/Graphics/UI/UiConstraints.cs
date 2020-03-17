namespace Tortuga.Graphics.UI
{
    public enum ConstraintType
    {
        Relative,
        Absolute,
        Percent,
        Max
    }
    public class Constraint
    {
        public ConstraintType Type;
        public float Value;

        public Constraint()
        {
            this.Type = ConstraintType.Relative;
            this.Value = 0.0f;
        }
        public Constraint(ConstraintType type, float value = 0)
        {
            this.Type = type;
            this.Value = value;
        }
    }

    public class RelativeConstraint : Constraint
    {
        public RelativeConstraint(float value)
        {
            this.Type = ConstraintType.Relative;
            this.Value = value;
        }
    }

    public class AbsoluteConstraint : Constraint
    {
        public AbsoluteConstraint(float value)
        {
            this.Type = ConstraintType.Absolute;
            this.Value = value;
        }
    }
    public class PercentConstraint : Constraint
    {
        public PercentConstraint(float value)
        {
            this.Type = ConstraintType.Percent;
            this.Value = value;
        }
    }

    public class MaxConstraint : Constraint
    {
        public MaxConstraint(float value)
        {
            this.Type = ConstraintType.Max;
            this.Value = value;
        }
    }

    public class UiConstraints
    {
        public Constraint X = new Constraint();
        public Constraint Y = new Constraint();
        public Constraint Width = new Constraint();
        public Constraint Height = new Constraint();
    }
}