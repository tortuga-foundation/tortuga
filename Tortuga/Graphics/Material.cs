using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    public class Material
    {
        private struct DescriptorSetObject
        {
            public DescriptorSetLayout Layout;
            public DescriptorSetPool Pool;
            public DescriptorSetPool.DescriptorSet Set;
            public List<Buffer> Buffers;
            public List<API.Image> Images;
            public List<ImageView> ImageViews;
            public List<Sampler> Samplers;
        }

        public PipelineInputBuilder InputBuilder;

        public static Material[] GetAllMaterials => _fullMaterialList.ToArray();

        internal Pipeline ActivePipeline => _pipeline;
        internal API.Buffer InstanceBuffers => _instanceBuffer;
        public bool IsInstanced => _isInstanced;
        internal DescriptorSetPool.DescriptorSet[] DescriptorSets
        {
            get
            {
                var sets = new List<DescriptorSetPool.DescriptorSet>();
                foreach (var obj in _descriptorMapper.Values)
                    sets.Add(obj.Set);
                return sets.ToArray();
            }
        }

        private Graphics.Shader _shader;
        private Pipeline _pipeline;
        private Dictionary<string, DescriptorSetObject> _descriptorMapper;
        private bool _isDirty;
        private API.Buffer _instanceBuffer;
        private static List<Material> _fullMaterialList = new List<Material>();
        private bool _isInstanced = false;

        public Material(
            Graphics.Shader shader,
            bool isInstanced = false
        )
        {

            _shader = shader;
            _descriptorMapper = new Dictionary<string, DescriptorSetObject>();
            _isInstanced = isInstanced;

            InputBuilder = new PipelineInputBuilder();
            var vertexBinding = new PipelineInputBuilder.BindingElement
            {
                Type = PipelineInputBuilder.BindingElement.BindingType.Vertex,
                Elements = new PipelineInputBuilder.AttributeElement[]
                {
                    new PipelineInputBuilder.AttributeElement(
                        PipelineInputBuilder.AttributeElement.FormatType.Float3
                    ),
                    new PipelineInputBuilder.AttributeElement(
                        PipelineInputBuilder.AttributeElement.FormatType.Float2
                    ),
                    new PipelineInputBuilder.AttributeElement(
                        PipelineInputBuilder.AttributeElement.FormatType.Float3
                    ),
                }
            };
            if (isInstanced)
            {
                InputBuilder.Bindings = new PipelineInputBuilder.BindingElement[]{
                    vertexBinding,
                    new PipelineInputBuilder.BindingElement
                    {
                        Type = PipelineInputBuilder.BindingElement.BindingType.Instance,
                        Elements = new PipelineInputBuilder.AttributeElement[]
                        {
                            new PipelineInputBuilder.AttributeElement(
                                PipelineInputBuilder.AttributeElement.FormatType.Float3
                            ),
                            new PipelineInputBuilder.AttributeElement(
                                PipelineInputBuilder.AttributeElement.FormatType.Float4
                            ),
                            new PipelineInputBuilder.AttributeElement(
                                PipelineInputBuilder.AttributeElement.FormatType.Float3
                            ),
                        }
                    }
                };
            }
            else
                InputBuilder.Bindings = new PipelineInputBuilder.BindingElement[] { vertexBinding };

            _isDirty = true;
            _fullMaterialList.Add(this);
        }
        ~Material()
        {
            _fullMaterialList.Remove(this);
        }

        internal List<BufferTransferObject> BuildInstanceBuffers(Components.RenderMesh[] meshes)
        {
            var transferObjects = new List<BufferTransferObject>();
            var bytes = new List<byte>();
            foreach (var mesh in meshes)
            {
                foreach (var b in PipelineInputBuilder.AttributeElement.GetBytes(mesh.Position))
                    bytes.Add(b);
                foreach (var b in PipelineInputBuilder.AttributeElement.GetBytes(mesh.Rotation))
                    bytes.Add(b);
                foreach (var b in PipelineInputBuilder.AttributeElement.GetBytes(mesh.Scale))
                    bytes.Add(b);
            }
            var totalByteSize = sizeof(byte) * bytes.Count;
            if (_instanceBuffer == null || _instanceBuffer.Size != totalByteSize)
            {
                _instanceBuffer = API.Buffer.CreateDevice(
                    System.Convert.ToUInt32(totalByteSize),
                    VkBufferUsageFlags.VertexBuffer
                );
            }
            transferObjects.Add(
                _instanceBuffer.SetDataGetTransferObject(bytes.ToArray())
            );
            return transferObjects;
        }

        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var totalDescriptorSets = new List<DescriptorSetLayout>();
            totalDescriptorSets.Add(Engine.Instance.CameraDescriptorLayout);
            totalDescriptorSets.Add(Engine.Instance.ModelDescriptorLayout);
            foreach (var l in _descriptorMapper.Values)
                totalDescriptorSets.Add(l.Layout);
            _pipeline = new Pipeline(
                totalDescriptorSets.ToArray(),
                _shader.Vertex,
                _shader.Fragment,
                InputBuilder.BindingDescriptions,
                InputBuilder.AttributeDescriptions
            );
            _isDirty = false;
        }

        public void UpdateShaders(Graphics.Shader shader)
        {
            _shader = shader;
            _isDirty = true;
        }

        public void CreateUniformData(string key, uint[] byteSizes)
        {
            if (_descriptorMapper.ContainsKey(key))
                return;

            var createInfos = new List<DescriptorSetCreateInfo>();
            foreach (var byteSize in byteSizes)
            {
                createInfos.Add(
                    new DescriptorSetCreateInfo
                    {
                        stage = VkShaderStageFlags.All,
                        type = VkDescriptorType.UniformBuffer
                    }
                );
            }

            var layout = new DescriptorSetLayout(createInfos.ToArray());
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var buffers = new List<Buffer>();
            foreach (var byteSize in byteSizes)
            {
                buffers.Add(
                    Buffer.CreateDevice(
                        byteSize,
                        VkBufferUsageFlags.UniformBuffer
                    )
                );
            }
            set.BuffersUpdate(buffers.ToArray());

            _descriptorMapper.Add(
                key, new DescriptorSetObject
                {
                    Layout = layout,
                    Pool = pool,
                    Set = set,
                    Buffers = buffers
                }
            );
            _isDirty = true;
        }
        public async Task UpdateUniformDataArray<T>(string key, int binding, T[] data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffers[binding].SetDataWithStaging(data);
        }
        public void CreateUniformData<A>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>())
            };
            CreateUniformData(key, sizes);
        }
        public void CreateUniformData<A, B>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<B>())
            };
            CreateUniformData(key, sizes);
        }
        public void CreateUniformData<A, B, C>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<B>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<C>())
            };
            CreateUniformData(key, sizes);
        }
        public async Task UpdateUniformData<T>(string key, int binding, T data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffers[binding].SetDataWithStaging(new T[] { data });
        }

        public void CreateSampledImage(string key, uint[] mipLevels)
        {
            if (_descriptorMapper.ContainsKey(key))
                return;

            var createInfo = new List<DescriptorSetCreateInfo>();
            foreach (var mipLevel in mipLevels)
            {
                createInfo.Add(
                   new DescriptorSetCreateInfo
                   {
                       stage = VkShaderStageFlags.All,
                       type = VkDescriptorType.CombinedImageSampler
                   }
               );
            }

            var layout = new DescriptorSetLayout(createInfo.ToArray());
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var images = new List<API.Image>();
            var views = new List<ImageView>();
            var samplers = new List<Sampler>();
            foreach (var mipLevel in mipLevels)
            {
                var image = new API.Image(1, 1, VkFormat.R8g8b8a8Srgb,
                    VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                    mipLevel
                );
                images.Add(image);
                views.Add(
                    new API.ImageView(
                        image,
                        VkImageAspectFlags.Color
                    )
                );
                samplers.Add(new API.Sampler());
            }
            set.SampledImageUpdate(views.ToArray(), samplers.ToArray());
            _descriptorMapper.Add(key, new DescriptorSetObject
            {
                Layout = layout,
                Pool = pool,
                Set = set,
                Images = images,
                ImageViews = views,
                Samplers = samplers
            });
            _isDirty = true;
        }
        public async Task UpdateSampledImage(string key, int binding, Image image)
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;

            var obj = _descriptorMapper[key];
            if (image.Width != obj.Images[binding].Width || image.Height != obj.Images[binding].Height)
            {
                obj.Images[binding] = new API.Image(
                    image.Width, image.Height,
                    VkFormat.R8g8b8a8Srgb,
                    VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                    obj.Images[binding].MipLevel
                );
                obj.ImageViews[binding] = new ImageView(obj.Images[binding], VkImageAspectFlags.Color);
                obj.Set.SampledImageUpdate(
                    obj.ImageViews[binding],
                    obj.Samplers[binding],
                    0,
                    System.Convert.ToUInt32(binding)
                );
                _descriptorMapper[key] = obj;
            }

            var pixelData = new ShaderPixel[image.Pixels.Length];
            for (int i = 0; i < pixelData.Length; i++)
            {
                var rawPixel = image.Pixels[i];
                pixelData[i] = new ShaderPixel
                {
                    R = rawPixel.R,
                    G = rawPixel.G,
                    B = rawPixel.B,
                    A = rawPixel.A
                };
            }

            var staging = Buffer.CreateHost(
                System.Convert.ToUInt32(Unsafe.SizeOf<ShaderPixel>() * image.Width * image.Height),
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst
            );

            var fence = new Fence();
            var commandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            var command = commandPool.AllocateCommands()[0];
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.TransferImageLayout(
                obj.Images[binding],
                VkImageLayout.Undefined,
                VkImageLayout.TransferDstOptimal,
                0
            );
            command.BufferToImage(staging, obj.Images[binding]);
            for (uint i = 0; i < obj.Images[binding].MipLevel - 1; i++)
            {
                command.TransferImageLayout(
                    obj.Images[binding],
                    VkImageLayout.TransferDstOptimal,
                    VkImageLayout.TransferSrcOptimal,
                    i
                );
                command.TransferImageLayout(
                    obj.Images[binding],
                    VkImageLayout.Undefined,
                    VkImageLayout.TransferDstOptimal,
                    i + 1
                );
                command.BlitImage(
                    obj.Images[binding].ImageHandle,
                    0, 0, obj.Images[binding].Width, obj.Images[binding].Height,
                    i,
                    obj.Images[binding].ImageHandle,
                    0, 0, obj.Images[binding].Width, obj.Images[binding].Height,
                    i + 1
                );
                command.TransferImageLayout(
                    obj.Images[binding],
                    VkImageLayout.TransferSrcOptimal,
                    VkImageLayout.ShaderReadOnlyOptimal,
                    i
                );
            }
            command.TransferImageLayout(
                obj.Images[binding],
                VkImageLayout.TransferDstOptimal,
                VkImageLayout.ShaderReadOnlyOptimal,
                obj.Images[binding].MipLevel - 1
            );
            command.End();

            await Task.Run(() =>
            {
                staging.SetData(pixelData);
                command.Submit(
                    Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                    null, null,
                    fence
                );
                fence.Wait();
            });
        }
        public async Task<T> GetUniformData<T>(string key, int binding) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return (await _descriptorMapper[key].Buffers[binding].GetDataWithStaging<T>())[0];
        }
        internal BufferTransferObject UpdateUniformDataSemaphore<T>(string key, int binding, T data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return _descriptorMapper[key].Buffers[binding].SetDataGetTransferObject(new T[] { data });
        }

        private static Material _cachedErrorMaterial;
        public static Material ErrorMaterial
        {
            get
            {
                if (_cachedErrorMaterial == null)
                    _cachedErrorMaterial = new Material(
                        Graphics.Shader.Load(
                            "Assets/Shaders/Error/Error.vert",
                            "Assets/Shaders/Error/Error.frag"
                        )
                    );
                return _cachedErrorMaterial;
            }
        }
    }
}