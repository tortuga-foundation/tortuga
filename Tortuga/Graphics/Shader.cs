

namespace Tortuga.Graphics
{
    public class Shader
    {
        internal API.Shader Vertex => _vertex;
        internal API.Shader Fragment => _fragment;

        private API.Shader _vertex;
        private API.Shader _fragment;

        public Shader(string vertex, string fragment)
        {
            _vertex = new API.Shader(vertex);
            _fragment = new API.Shader(fragment);
        }
        internal Shader(API.Shader vertex, API.Shader fragment)
        {
            _vertex = vertex;
            _fragment = fragment;
        }
    }
}