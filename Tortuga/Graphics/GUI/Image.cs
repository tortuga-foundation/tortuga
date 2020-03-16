namespace Tortuga.Graphics.GUI
{
    public class Image : Container
    {
        public Graphics.Image Background;

        public Image()
        {
            this.Background = Graphics.Image.SingleColor(System.Drawing.Color.White);
        }
    }
}