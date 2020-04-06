namespace Tortuga.Graphics.UI.Base
{
    /// <summary>
    /// Contains resources used to render user interface
    /// This can include, fonts, materials e.t.c.
    /// </summary>
    public static class UiResources
    {
        /// <summary>
        /// Stores all the default materials used by the user interface system
        /// </summary>
        public static class Materials
        {
            /// <summary>
            /// Standard block material used for UiRenderable
            /// </summary>
            public static UiMaterial Block => _block;
            private static UiMaterial _block = new UiMaterial(
                Shader.Load(
                    "Assets/Shaders/UI/Base.vert",
                    "Assets/Shaders/UI/Base.frag"
                )
            );

            /// <summary>
            /// Text material used by UiText for rendering text
            /// </summary>
            public static UiMaterial Text => _text;
            private static UiMaterial _text = new UiMaterial(
                Shader.Load(
                    "Assets/Shaders/UI/Text.vert",
                    "Assets/Shaders/UI/Text.frag"
                ),
                new PipelineInputBuilder(
                    new PipelineInputBuilder.BindingElement[]
                    {
                    new PipelineInputBuilder.BindingElement
                    {
                        Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                        Elements = new PipelineInputBuilder.AttributeElement[]
                        {
                            new PipelineInputBuilder.AttributeElement(
                                PipelineInputBuilder.AttributeElement.FormatType.Float2
                            ),
                            new PipelineInputBuilder.AttributeElement(
                                PipelineInputBuilder.AttributeElement.FormatType.Float2
                            )
                        }
                    }
                    }
                )
            );
        }

        /// <summary>
        /// Stores all the default fonts used by the user interface system
        /// </summary>
        public static class Font
        {
            /// <summary>
            /// Robot font used by the user interface
            /// </summary>
            public static UiFont Roboto
            {
                get
                {
                    if (_roboto == null)
                    {
                        var task = UiFont.Load("Assets/Fonts/Roboto.json");
                        task.Wait();
                        _roboto = task.Result;
                    }
                    return _roboto;
                }
            }
            private static UiFont _roboto;
        }
    }
}