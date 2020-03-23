namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// Horizontal layout for user interface
    /// </summary>
    public class UiHorizontalLayout : UiElement
    {
        /// <summary>
        /// How much space (in pixels) to have between each child element
        /// </summary>
        public float Spacing;

        /// <summary>
        /// Constructor for Ui Horizontal Layout
        /// </summary>
        public UiHorizontalLayout()
        {
            Spacing = 0.0f;
        }

        /// <summary>
        /// Updates the positions for Horizontal layout
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            base.UpdatePositionsWithConstraints();

            float offset = 0.0f;
            foreach (var child in this.Children)
            {
                child.PositionXConstraint = new PixelConstraint(offset);
                offset += child.Scale.X + Spacing;
            }
        }
    }
}