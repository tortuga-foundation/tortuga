namespace Tortuga.Graphics.UI
{
    /// <summary>
    /// text user interface element
    /// </summary>
    public class UiText : UiRenderable
    {
        /// <summary>
        /// font used for rendering text
        /// </summary>
        public UiFont Font
        {
            get => _font;
            set
            {
                _font = value;
                var task = _material.UpdateSampledImage("Font", 0, Font.Atlas);
                task.Wait();
            }
        }
        private UiFont _font = UiResources.Font.Roboto;

        /// <summary>
        /// Constructor for Ui Text
        /// </summary>
        public UiText()
        {
            _material = UiResources.Materials.Text;
            _material.CreateSampledImage("Font", new uint[] { 1 });
            var task = _material.UpdateSampledImage("Font", 0, Font.Atlas);
            task.Wait();
        }
    }
}