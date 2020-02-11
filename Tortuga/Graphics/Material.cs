using Tortuga.Graphics.API;

namespace Tortuga.Graphics
{
    public class Material
    {
        internal Shader Vertex => _vertex;
        internal Shader Fragment => _fragment;
        internal Pipeline ActivePipeline => _pipeline;

        private Shader _vertex;
        private Shader _fragment;
        private Pipeline _pipeline;

        public Material(string vertexShader, string fragmentShader)
        {
            _vertex = new Shader(vertexShader);
            _fragment = new Shader(fragmentShader);
            _pipeline = new Pipeline(new DescriptorSetLayout[0], _vertex, _fragment);
        }
    }
}