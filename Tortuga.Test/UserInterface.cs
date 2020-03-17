using Tortuga.Graphics.UI;

namespace Tortuga.Test
{
    public class UserInterface
    {
        public UserInterface()
        {
            var activityBar = new UiBlock(System.Drawing.Color.FromArgb(255, 10, 10, 10));
            activityBar.Constraints.X = new MaxConstraint(310.0f);
            activityBar.Constraints.Y = new RelativeConstraint(10.0f);
            activityBar.Constraints.Width = new RelativeConstraint(300.0f);
            activityBar.Constraints.Height = new MaxConstraint(20.0f);
            activityBar.BorderRadius = 20.0f;
        }
    }
}