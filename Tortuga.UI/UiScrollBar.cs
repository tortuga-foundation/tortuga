namespace Tortuga.UI
{
    /// <summary>
    /// Scroll bar's are used for scroll rects
    /// </summary>
    public class UiScrollBar : UiSlider
    {
        /// <summary>
        /// Set's up the slider to be used in vertial or horizontal mode
        /// </summary>
        /// <param name="type">Vertial or Horizontal</param>
        protected override void ChangeSliderType(TypeOfSlider type)
        {
            if (type == TypeOfSlider.Horizontal)
            {
                this.ScaleXConstraint = new PercentConstraint(1.0f);
                this.ScaleYConstraint = new PixelConstraint(5.0f);

                _thumb.PositionXConstraint = new PercentConstraint(0.0f);
                _thumb.PositionYConstraint = new PercentConstraint(0.0f);
                _thumb.ScaleXConstraint = new PixelConstraint(20.0f);
                _thumb.ScaleYConstraint = new PercentConstraint(1.0f);
                _thumb.BorderRadius = 2.0f;

                _slider.PositionXConstraint = new PixelConstraint(0.0f);
                _slider.PositionYConstraint = new PixelConstraint(0.0f);
                _slider.ScaleXConstraint = new PercentConstraint(1.0f);
                _slider.ScaleYConstraint = new PercentConstraint(1.0f);
                _slider.BorderRadius = 2.0f;
            }
            else if (type == TypeOfSlider.Vertical)
            {
                this.ScaleXConstraint = new PixelConstraint(5.0f);
                this.ScaleYConstraint = new PercentConstraint(1.0f);

                _thumb.PositionXConstraint = new PercentConstraint(0.0f);
                _thumb.PositionYConstraint = new PercentConstraint(0.0f);
                _thumb.ScaleXConstraint = new PercentConstraint(1.0f);
                _thumb.ScaleYConstraint = new PixelConstraint(20.0f);
                _thumb.BorderRadius = 2.0f;

                _slider.PositionXConstraint = new PixelConstraint(0.0f);
                _slider.PositionYConstraint = new PixelConstraint(0.0f);
                _slider.ScaleXConstraint = new PercentConstraint(1.0f);
                _slider.ScaleYConstraint = new PercentConstraint(1.0f);
                _slider.BorderRadius = 2.0f;
            }
        }
    }
}