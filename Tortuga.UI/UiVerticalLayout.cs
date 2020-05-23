using Tortuga.UI.Base;

namespace Tortuga.UI
{
    /// <summary>
    /// Vertical layout for user interface
    /// </summary>
    public class UiVerticalLayout : UiElement
    {
        /// <summary>
        /// How much space (in pixels) to have between each child element
        /// </summary>
        public float Spacing;

        /// <summary>
        /// Constructor for Ui Vertical Layout
        /// </summary>
        public UiVerticalLayout()
        {
            Spacing = 0.0f;
        }

        /// <summary>
        /// Updates the positions for vertical layout
        /// </summary>
        public override void UpdatePositionsWithConstraints()
        {
            base.UpdatePositionsWithConstraints();

            float offset = 0.0f;
            foreach (var child in this.Children)
            {
                child.PositionYConstraint = new PixelConstraint(offset);
                offset += child.Scale.Y + Spacing;
            }
        }
    }
}