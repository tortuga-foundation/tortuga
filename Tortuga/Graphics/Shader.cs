

using System.Collections.Generic;

namespace Tortuga.Graphics
{
    public class Shader
    {
        internal API.Shader Vertex => _vertex;
        internal API.Shader Fragment => _fragment;

        private API.Shader _vertex;
        private API.Shader _fragment;

        private static Dictionary<string, Shader> _compiledShaders = new Dictionary<string, Shader>();

        internal Shader(string vertex, string fragment)
        {
            _vertex = new API.Shader(vertex);
            _fragment = new API.Shader(fragment);
        }
        internal Shader(API.Shader vertex, API.Shader fragment)
        {
            _vertex = vertex;
            _fragment = fragment;
        }

        public static Shader Load(string vertex, string fragment)
        {
            var key = string.Format("{0}-{1}", vertex, fragment);
            if (_compiledShaders.ContainsKey(key))
                return _compiledShaders[key];
            else
            {
                _compiledShaders[key] = new Shader(vertex, fragment);
                return _compiledShaders[key];
            }
        }
    }
}