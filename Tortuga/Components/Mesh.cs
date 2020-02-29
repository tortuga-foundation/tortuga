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

        private Material _material;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Graphics.API.Buffer _vertexBuffer;
        private Graphics.API.Buffer _indexBuffer;
        private uint _indicesCount;
        private uint[] _indices;
        private Vertex[] _vertices;

        public async override Task OnEnable()
        {
            await Task.Run(() =>
            {
                if (_material == null)
                {
                    _material = new Material(new Graphics.Shader(
                        "Assets/Shaders/PBR/PBR.vert",
                        "Assets/Shaders/PBR/PBR.frag"
                    ));
                    _material.CreateUniformData<PBR>("pbr");
                    _material.CreateSampledImage("albedo", 1, 1);
                    _material.CreateSampledImage("normal", 1, 1);
                    _material.CreateSampledImage("metal", 1, 1);
                    _material.CreateSampledImage("roughness", 1, 1);
                    _material.CreateSampledImage("ao", 1, 1);

                    //copy data
                    var task = Task.Run(async () =>
                    {
                        await _material.UpdateUniformData("pbr", new PBR
                        {
                            EnableSmoothShading = 0
                        });
                        await _material.UpdateSampledImage(
                            "albedo",
                            new Graphics.Image("Assets/Images/Bricks/Bricks01_COL_1K.jpg")
                        );
                        await _material.UpdateSampledImage(
                            "normal",
                            new Graphics.Image("Assets/Images/Bricks/Bricks01_NRM_1K.jpg")
                        );
                        await _material.UpdateSampledImage(
                            "metal",
                            new Graphics.Image("Assets/Images/Bricks/Bricks01_GLOSS_1K.jpg")
                        );
                        await _material.UpdateSampledImage(
                            "roughness",
                            new Graphics.Image("Assets/Images/Bricks/Bricks01_REFL_1K.jpg")
                        );
                        await _material.UpdateSampledImage(
                            "ao",
                            new Graphics.Image("Assets/Images/Bricks/Bricks01_AO_1K.jpg")
                        );
                    });
                    task.Wait();
                }
                _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
                _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            });
        }
        public async Task SetVertices(Vertex[] vertices)
        {
            _vertices = vertices;
            _vertexBuffer = Graphics.API.Buffer.CreateDevice(
                Convert.ToUInt32(Unsafe.SizeOf<Vertex>() * vertices.Length),
                VkBufferUsageFlags.VertexBuffer
            );
            await _vertexBuffer.SetDataWithStaging(vertices);
        }
        public async Task SetIndices(uint[] indices)
        {
            this._indicesCount = Convert.ToUInt32(indices.Length);
            this._indices = indices;
            _indexBuffer = Graphics.API.Buffer.CreateDevice(
                Convert.ToUInt32(sizeof(uint) * indices.Length),
                VkBufferUsageFlags.IndexBuffer
            );
            await _indexBuffer.SetDataWithStaging(indices);
        }
        public async Task ComputeTangents()
        {
            _vertices = Vertex.ComputeTangents(_vertices, _indices);
            await _vertexBuffer.SetDataWithStaging(_vertices);
        }
        public Matrix4x4 ModelMatrix
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return Matrix4x4.Identity;

                return transform.ToMatrix;
            }
        }
        public Vector3 Position
        {
            get
            {
                var transform = MyEntity.GetComponent<Transform>();
                if (transform == null)
                    return Vector3.Zero;

                return transform.Position;
            }
        }
    }
}