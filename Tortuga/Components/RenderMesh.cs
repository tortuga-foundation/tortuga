using System;
using System.Threading.Tasks;
using Vulkan;
using Tortuga.Graphics;
using Tortuga.Graphics.API;
using System.Numerics;
using System.Collections.Generic;

namespace Tortuga.Components
{
    public class RenderMesh : Core.BaseComponent
    {
        public Material Material
        {
            set
            {
                _material = value;
                Task.Run(() => SetMesh(_mesh));
            }
            get => _material;
        }
        public Graphics.Mesh Mesh => _mesh;
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


        internal CommandPool.Command RenderCommand => _renderCommand;
        internal Dictionary<uint, Graphics.API.Buffer> VertexBuffers => _vertexBuffers;
        internal Graphics.API.Buffer IndexBuffer => _indexBuffer;
        internal uint IndicesCount => Mesh.IndicesLength;

        private Graphics.Material _material;
        private Graphics.Mesh _mesh;
        private CommandPool _renderCommandPool;
        private CommandPool.Command _renderCommand;
        private Dictionary<uint, Graphics.API.Buffer> _vertexBuffers;
        private Graphics.API.Buffer _indexBuffer;

        public async override Task OnEnable()
        {
            _vertexBuffers = new Dictionary<uint, Graphics.API.Buffer>();
            await Task.Run(() =>
            {
                if (Material == null)
                    Material = Material.ErrorMaterial;

                _renderCommandPool = new CommandPool(Engine.Instance.MainDevice.GraphicsQueueFamily);
                _renderCommand = _renderCommandPool.AllocateCommands(VkCommandBufferLevel.Secondary)[0];
            });
        }

        public async Task SetMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

            if (mesh.IndicesLength == 0 || mesh.Vertices.Length == 0)
                return;

            //index buffer
            if (_mesh == null || _mesh.IndicesLength != mesh.IndicesLength)
            {
                _indexBuffer = Graphics.API.Buffer.CreateDevice(
                    sizeof(ushort) * mesh.IndicesLength,
                    VkBufferUsageFlags.IndexBuffer | VkBufferUsageFlags.TransferDst
                );
            }

            if (_mesh == null || _mesh.Indices != mesh.Indices)
                await _indexBuffer.SetDataWithStaging(mesh.Indices);

            //vertex buffer
            for (uint i = 0; i < Material.InputBuilder.Bindings.Length; i++)
            {
                var binding = Material.InputBuilder.Bindings[i];
                if (binding.Type == Graphics.PipelineInputBuilder.BindingElement.BindingType.Vertex)
                {
                    var bytes = new List<byte>();
                    foreach (var vertex in mesh.Vertices)
                    {
                        foreach (var attribute in binding.Elements)
                        {
                            if (attribute.Content == Graphics.PipelineInputBuilder.AttributeElement.ContentType.VertexPosition)
                            {
                                foreach (var b in attribute.GetBytes(vertex.Position))
                                    bytes.Add(b);
                            }
                            else if (attribute.Content == Graphics.PipelineInputBuilder.AttributeElement.ContentType.VertexUV)
                            {
                                foreach (var b in attribute.GetBytes(vertex.TextureCoordinates))
                                    bytes.Add(b);
                            }
                            else if (attribute.Content == Graphics.PipelineInputBuilder.AttributeElement.ContentType.VertexNormal)
                            {
                                foreach (var b in attribute.GetBytes(vertex.Normal))
                                    bytes.Add(b);
                            }
                        }
                    }

                    _vertexBuffers.Add(i,
                        Graphics.API.Buffer.CreateDevice(
                            System.Convert.ToUInt32(sizeof(byte) * bytes.Count),
                            VkBufferUsageFlags.VertexBuffer
                        )
                    );
                    await _vertexBuffers[i].SetDataWithStaging(mesh.Vertices);
                }
            }

            _mesh = mesh;
        }

        internal Task<CommandPool.Command> RecordRenderCommand(Components.Camera camera)
        {
            this.RenderCommand.Begin(VkCommandBufferUsageFlags.RenderPassContinue, camera.Framebuffer, 0);
            this.RenderCommand.SetViewport(
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Width * camera.Viewport.X)),
                System.Convert.ToInt32(System.Math.Round(Engine.Instance.MainWindow.Height * camera.Viewport.Y)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.x * camera.Viewport.Width)),
                System.Convert.ToUInt32(System.Math.Round(camera.Resolution.y * camera.Viewport.Width))
            );

            var descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            descriptorSets.Add(camera.CameraDescriptorSet);
            foreach (var d in this.Material.DescriptorSets)
                descriptorSets.Add(d);
            this.Material.ReCompilePipeline();
            this.RenderCommand.BindPipeline(
                this.Material.ActivePipeline,
                VkPipelineBindPoint.Graphics,
                descriptorSets.ToArray()
            );
            foreach (var vertexBuffer in this.VertexBuffers)
                this.RenderCommand.BindVertexBuffer(vertexBuffer.Value, vertexBuffer.Key);
            this.RenderCommand.BindIndexBuffer(this.IndexBuffer);
            this.RenderCommand.DrawIndexed(this.IndicesCount);
            this.RenderCommand.End();
            return Task.FromResult(this.RenderCommand);
        }
        internal Components.Light.FullShaderInfo RenderingLights(Components.Light[] lights)
        {
            System.Array.Sort(lights, (Components.Light left, Components.Light right) =>
            {
                var leftDist = Vector3.Distance(left.Position, this.Position);
                var rightDist = Vector3.Distance(right.Position, this.Position);
                return System.Convert.ToInt32(System.MathF.Round(leftDist - rightDist));
            });
            if (lights.Length > 10)
                System.Array.Resize(ref lights, 10);
            var infoList = new List<Components.Light.LightShaderInfo>();
            foreach (var l in lights)
                infoList.Add(l.BuildShaderInfo);
            for (int i = infoList.Count; i < 10; i++)
                infoList.Add(new Components.Light.LightShaderInfo());
            return new Components.Light.FullShaderInfo
            {
                Count = lights.Length,
                Light0 = infoList[0],
                Light1 = infoList[1],
                Light2 = infoList[2],
                Light3 = infoList[3],
                Light4 = infoList[4],
                Light5 = infoList[5],
                Light6 = infoList[6],
                Light7 = infoList[7],
                Light8 = infoList[8],
                Light9 = infoList[9]
            };
        }
    }
}