using System.Collections.Generic;
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
        public ushort[] Indices
        {
            set
            {
                _indices = value;
                _indicesLength = System.Convert.ToUInt32(value.Length);
            }
            get => _indices;
        }
        private ushort[] _indices;
        public uint IndicesLength => _indicesLength;
        private uint _indicesLength;

        public Vertex[] Vertices
        {
            set => _vertices = value;
            get => _vertices;
        }
        private Vertex[] _vertices;
        public static Mesh[] GetAllMeshes => _allMeshes.ToArray();
        private static List<Mesh> _allMeshes = new List<Mesh>();
    
        public Mesh()
        {
            _allMeshes.Add(this);
        }
        ~Mesh()
        {
            _allMeshes.Remove(this);
        }
    }
}