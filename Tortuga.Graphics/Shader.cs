using System.Collections.Generic;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Shader object used for rendering
    /// </summary>
    public class Shader
    {
        internal API.Shader Vertex => _vertex;
        internal API.Shader Fragment => _fragment;

        private API.Shader _vertex;
        private API.Shader _fragment;

        private static Dictionary<string, Shader> _compiledShaders = new Dictionary<string, Shader>();

        private Shader(string vertex, string fragment)
        {
            _vertex = new API.Shader(vertex);
            _fragment = new API.Shader(fragment);
        }
        internal Shader(API.Shader vertex, API.Shader fragment)
        {
            _vertex = vertex;
            _fragment = fragment;
        }

        /// <summary>
        /// Load a shader object using uncompiled files
        /// </summary>
        /// <param name="vertex">path to vertex shader file</param>
        /// <param name="fragment">path to fragment shader file</param>
        /// <returns>Shader object that can be used in a pipeline</returns>
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