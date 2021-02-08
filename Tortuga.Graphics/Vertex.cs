#pragma warning disable CS1591
using System;
using System.Numerics;

namespace Tortuga.Graphics
{
    [Serializable]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TextureCoordinates;
        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 BiTangent;
    }
}