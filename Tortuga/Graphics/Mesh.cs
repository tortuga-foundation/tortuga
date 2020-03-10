using System.Numerics;

namespace Tortuga.Graphics
{
    [System.Serializable]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinates;
        public Vector3 Normal;
    }


    public class Mesh
    {
        public uint[] Indices
        {
            set
            {
                _indices = value;
                _indicesLength = System.Convert.ToUInt32(value.Length);
            }
            get => _indices;
        }
        private uint[] _indices;
        public uint IndicesLength => _indicesLength;
        private uint _indicesLength;

        public Vertex[] Vertices
        {
            set => _vertices = value;
            get => _vertices;
        }
        private Vertex[] _vertices;
    }
}