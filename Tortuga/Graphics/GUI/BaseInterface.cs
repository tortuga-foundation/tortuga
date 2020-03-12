using Vulkan;
using Tortuga.Graphics.API;
using System.Numerics;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Tortuga.Graphics.GUI
{
    public class BaseInterface
    {
        public struct UIVertex
        {
            public Vector2 Position;
            public Vector2 TextureCoordinates;
        }

        internal API.Buffer VertexBuffer;
        internal API.Buffer IndexBuffer;
        internal uint IndexCount;
        internal List<CommandPool.Command> TransferCommands
        {
            get
            {
                if (VertexBuffer == null || VertexBuffer.Size != _vertices.Length * Unsafe.SizeOf<UIVertex>())
                {
                    VertexBuffer = API.Buffer.CreateDevice(
                        System.Convert.ToUInt32(_vertices.Length * Unsafe.SizeOf<UIVertex>()),
                        VkBufferUsageFlags.VertexBuffer
                    );
                }
                if (IndexBuffer == null || IndexBuffer.Size != _indices.Length * sizeof(ushort))
                {
                    IndexBuffer = API.Buffer.CreateDevice(
                        System.Convert.ToUInt32(_indices.Length * sizeof(ushort)),
                        VkBufferUsageFlags.IndexBuffer
                    );
                }

                var transferCommands = new List<CommandPool.Command>();
                transferCommands.Add(
                    VertexBuffer.SetDataGetTransferObject(
                        _vertices
                    ).TransferCommand
                );
                transferCommands.Add(
                    IndexBuffer.SetDataGetTransferObject(
                        _indices
                    ).TransferCommand
                );
                _isDirty = false;
                return transferCommands;
            }
        }

        public bool IsDirty => _isDirty;
        public UIVertex[] Vertices
        {
            set
            {
                _vertices = value;
                _isDirty = true;
            }
            get => _vertices;
        }
        public ushort[] Indices
        {
            set
            {
                _indices = value;
                IndexCount = System.Convert.ToUInt32(value.Length);
                _isDirty = true;
            }
            get => _indices;
        }

        private UIVertex[] _vertices;
        private ushort[] _indices;

        private CommandPool _transferCommandPool;
        private CommandPool.Command _transferCommand;
        private bool _isDirty;

        public BaseInterface()
        {
            _transferCommandPool = new CommandPool(Engine.Instance.MainDevice.TransferQueueFamily);
            _transferCommand = _transferCommandPool.AllocateCommands()[0];
            Vertices = new UIVertex[]{
                new UIVertex
                {
                    Position = new Vector2(0, 0),
                    TextureCoordinates = new Vector2(0, 0)
                },
                new UIVertex
                {
                    Position = new Vector2(100, 0),
                    TextureCoordinates = new Vector2(0, 0)
                },
                new UIVertex
                {
                    Position = new Vector2(0, 100),
                    TextureCoordinates = new Vector2(0, 0)
                },
                new UIVertex
                {
                    Position = new Vector2(100, 100),
                    TextureCoordinates = new Vector2(0, 0)
                },
            };
            Indices = new ushort[]{
                0, 1, 2,
                2, 1, 3
            };
        }
    }
}