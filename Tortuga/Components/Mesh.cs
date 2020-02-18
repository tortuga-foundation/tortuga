using System;
using System.Threading.Tasks;
using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Tortuga.Components
{
    public class Mesh : Core.BaseComponent
    {
        public Material ActiveMaterial
        {
            set { _material = value; }
            get { return _material; }
        }
        internal CommandPool.Command RenderCommand => _renderCommand;
        internal Graphics.API.Buffer VertexBuffer => _vertexBuffer;
        internal Graphics.API.Buffer IndexBuffer => _indexBuffer;
        internal uint IndicesCount => _indicesCount;
        public bool IsStatic
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return false;

                return transform.IsStatic;
            }
        }
        public bool DisableTangents = false;
        public bool IsVerticesDirty => _verticesDirty;

        private Material _material;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Graphics.API.Buffer _vertexBuffer;
        private Graphics.API.Buffer _indexBuffer;
        private uint _indicesCount;
        private uint[] _indices;
        private Vertex[] _vertices;
        private bool _verticesDirty;

        public async override Task OnEnable()
        {
            await Task.Run(() =>
            {
                if (_material == null)
                    _material = Tortuga.Global.Instance.Materials["Simple"];
                _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
                _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            });
        }
        public async Task SetVertices(Vertex[] vertices)
        {
            _vertices = vertices;
            _vertexBuffer = Graphics.API.Buffer.CreateDevice(
                Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * vertices.Length),
                VkBufferUsageFlags.VertexBuffer | VkBufferUsageFlags.TransferDst
            );
            await _vertexBuffer.SetDataWithStaging(vertices);
            _verticesDirty = true;
        }
        public async Task SetIndices(uint[] indices)
        {
            this._indicesCount = Convert.ToUInt32(indices.Length);
            this._indices = indices;
            _indexBuffer = Graphics.API.Buffer.CreateDevice(
                Convert.ToUInt32(sizeof(uint) * indices.Length),
                VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst
            );
            await _indexBuffer.SetDataWithStaging(indices);
        }
        public Matrix4x4 ModelMatrix
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return Matrix4x4.Identity;

                var mat = Matrix4x4.Identity;
                mat *= Matrix4x4.CreateScale(transform.Scale);
                mat *= Matrix4x4.CreateFromQuaternion(transform.Rotation);
                mat *= Matrix4x4.CreateTranslation(transform.Position);
                return mat;
            }
        }
        public async Task ComputeTangents()
        {
            if (DisableTangents)
                return;
            //compute tangent & bi tangents
            for (uint i = 0; i < _indices.Length; i += 3)
            {
                var v0 = _vertices[_indices[i + 0]];
                var v1 = _vertices[_indices[i + 1]];
                var v2 = _vertices[_indices[i + 2]];

                var edge1 = v1.Position - v0.Position;
                var edge2 = v2.Position - v0.Position;

                float deltaU1 = v1.TextureCoordinates.X - v0.TextureCoordinates.X;
                float deltaV1 = v1.TextureCoordinates.Y - v0.TextureCoordinates.Y;
                float deltaU2 = v2.TextureCoordinates.X - v0.TextureCoordinates.X;
                float deltaV2 = v2.TextureCoordinates.Y - v0.TextureCoordinates.Y;

                float f = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);
                Vector3 tangent, bitangent;
                tangent.X = f * (deltaV2 * edge1.X - deltaV1 * edge2.X);
                tangent.Y = f * (deltaV2 * edge1.Y - deltaV1 * edge2.Y);
                tangent.Z = f * (deltaV2 * edge1.Z - deltaV1 * edge2.Z);

                bitangent.X = f * (-deltaU2 * edge1.X - deltaU1 * edge2.X);
                bitangent.Y = f * (-deltaU2 * edge1.Y - deltaU1 * edge2.Y);
                bitangent.Z = f * (-deltaU2 * edge1.Z - deltaU1 * edge2.Z);

                _vertices[_indices[i + 0]].Tangent += tangent;
                _vertices[_indices[i + 1]].Tangent += tangent;
                _vertices[_indices[i + 2]].Tangent += tangent;

                _vertices[_indices[i + 0]].BiTangent += bitangent;
                _vertices[_indices[i + 1]].BiTangent += bitangent;
                _vertices[_indices[i + 2]].BiTangent += bitangent;
            }
            //normalize tangents
            for (uint i = 0; i < _vertices.Length; i++)
            {
                _vertices[i].Tangent = Vector3.Normalize(_vertices[i].Tangent);
                _vertices[i].BiTangent = Vector3.Normalize(_vertices[i].BiTangent);
            }
            await _vertexBuffer.SetDataWithStaging(_vertices);
            _verticesDirty = false;
        }
    }
}