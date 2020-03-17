using Tortuga.Graphics.UI;

namespace Tortuga.Test
{
    public class UserInterface
    {
        public UserInterface()
        {
            var activityBar = new Graphics.UI.UiBlock(System.Drawing.Color.FromArgb(255, 10, 10, 10));
            activityBar.Constraints.X = new Graphics.UI.MaxConstraint(310.0f);
            activityBar.Constraints.Y = new Graphics.UI.RelativeConstraint(10.0f);
            activityBar.Constraints.Width = new Graphics.UI.RelativeConstraint(300.0f);
            activityBar.Constraints.Height = new Graphics.UI.MaxConstraint(20.0f);
            activityBar.BorderRadius = 20.0f;
        }
    }
}