using System.Numerics;

namespace Tortuga.Graphics.UserInterface
{
    public class UserInterface
    {
        private Canvas _canvas;

        public UserInterface(Canvas canvas)
        {
            this._canvas = canvas;
        }

        public Vector2 PositionPixel;
        public Vector2 PositionPercent
        {
            get => new Vector2(
                PositionPixel.X / _canvas.Scale.X,
                PositionPixel.Y / _canvas.Scale.Y
            );
            set => new Vector2(
                PositionPixel.X * _canvas.Scale.X,
                PositionPixel.Y * _canvas.Scale.Y
            );
        }

        public Vector2 ScalePixel;
        public Vector2 ScalePercent
        {
             get => new Vector2(
                ScalePixel.X / _canvas.Scale.X,
                ScalePixel.Y / _canvas.Scale.Y
            );
            set => new Vector2(
                ScalePixel.X * _canvas.Scale.X,
                ScalePixel.Y * _canvas.Scale.Y
            );
        }
    
        public int IndexZ = 0;
        public float BorderRadius = 0;
        public Graphics.Image Background;
        public Shadow Shadow;
    }
}