using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    public class Material
    {
        public enum ShaderDataType
        {
            Data,
            Image
        };

        private struct DescriptorSetObject
        {
            public DescriptorSetLayout Layout;
            public DescriptorSetPool Pool;
            public DescriptorSetPool.DescriptorSet Set;
            public Buffer Buffer;
            public API.Image Image;
            public ImageView ImageView;
            public Sampler Sampler;
        }
        private struct VulkanPixel
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        };

        public Matrix4x4 Model;

        internal Pipeline ActivePipeline => _pipeline;
        internal bool UsingLighting => _usingLighting;
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
        private bool _usingLighting;

        public Material(Graphics.Shader shader, bool includeLighting = true)
        {
            _usingLighting = includeLighting;

            _shader = shader;
            _descriptorMapper = new Dictionary<string, DescriptorSetObject>();

            //model matrix
            CreateUniformData<Matrix4x4>("MODEL");

            if (_usingLighting)
                CreateUniformData<Systems.RenderingSystem.LightShaderInfo>("LIGHT");
            _isDirty = true;
        }

        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var totalDescriptorSets = new List<DescriptorSetLayout>();
            totalDescriptorSets.Add(Engine.Instance.CameraDescriptorLayout);
            foreach (var l in _descriptorMapper.Values)
                totalDescriptorSets.Add(l.Layout);

            _pipeline = new Pipeline(
                totalDescriptorSets.ToArray(),
                _shader.Vertex,
                _shader.Fragment
            );
            _isDirty = false;
        }

        public void UpdateShaders(Graphics.Shader shader)
        {
            _shader = shader;
            _isDirty = true;
        }

        public void CreateUniformData<T>(string key) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key))
                return;

            var layout = new DescriptorSetLayout(
                new DescriptorSetCreateInfo[]
                {
                    new DescriptorSetCreateInfo{
                        stage = VkShaderStageFlags.All,
                        type = VkDescriptorType.UniformBuffer
                    }
                }
            );
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var buffer = Buffer.CreateDevice(
                System.Convert.ToUInt32(Unsafe.SizeOf<T>()),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst
            );
            set.BuffersUpdate(buffer);

            _descriptorMapper.Add(
                key, new DescriptorSetObject
                {
                    Layout = layout,
                    Pool = pool,
                    Set = set,
                    Buffer = buffer
                }
            );
            _isDirty = true;
        }
        public async Task UpdateUniformData<T>(string key, T data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffer.SetDataWithStaging(new T[] { data });
        }

        public void CreateSampledImage(string key, uint width, uint height, uint mipLevel = 1)
        {
            if (_descriptorMapper.ContainsKey(key))
                return;

            var layout = new DescriptorSetLayout(
                new DescriptorSetCreateInfo[]
                {
                    new DescriptorSetCreateInfo
                    {
                        stage = VkShaderStageFlags.All,
                        type = VkDescriptorType.CombinedImageSampler
                    }
                }
            );
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var image = new API.Image(
                width, height,
                VkFormat.R8g8b8a8Srgb,
                VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                mipLevel
            );
            var imageView = new API.ImageView(
                image,
                VkImageAspectFlags.Color
            );
            var sampler = new API.Sampler();
            set.SampledImageUpdate(imageView, sampler);
            _descriptorMapper.Add(key, new DescriptorSetObject
            {
                Layout = layout,
                Pool = pool,
                Set = set,
                Image = image,
                ImageView = imageView,
                Sampler = sampler
            });
            _isDirty = true;
        }
        public async Task UpdateSampledImage(string key, Image image)
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;

            var obj = _descriptorMapper[key];
            if (image.Width != obj.Image.Width || image.Height != obj.Image.Height)
            {
                obj.Image = new API.Image(
                    image.Width, image.Height,
                    VkFormat.R8g8b8a8Srgb,
                    VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                    obj.Image.MipLevel
                );
                obj.ImageView = new ImageView(obj.Image, VkImageAspectFlags.Color);
                obj.Set.SampledImageUpdate(obj.ImageView, obj.Sampler);
                _descriptorMapper[key] = obj;
            }

            var pixelData = new VulkanPixel[image.Pixels.Length];
            for (int i = 0; i < pixelData.Length; i++)
            {
                var rawPixel = image.Pixels[i];
                pixelData[i] = new VulkanPixel
                {
                    R = rawPixel.R,
                    G = rawPixel.B,
                    B = rawPixel.G,
                    A = rawPixel.A
                };
            }

            var staging = Buffer.CreateHost(
                System.Convert.ToUInt32(Unsafe.SizeOf<VulkanPixel>() * image.Width * image.Height),
                VkBufferUsageFlags.TransferSrc | VkBufferUsageFlags.TransferDst
            );

            var fence = new Fence();
            var commandPool = new CommandPool(
                Engine.Instance.MainDevice.GraphicsQueueFamily
            );
            var command = commandPool.AllocateCommands()[0];
            command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
            command.TransferImageLayout(
                obj.Image,
                VkImageLayout.Undefined,
                VkImageLayout.TransferDstOptimal,
                0
            );
            command.BufferToImage(staging, obj.Image);
            for (uint i = 0; i < obj.Image.MipLevel - 1; i++)
            {
                command.TransferImageLayout(
                    obj.Image,
                    VkImageLayout.TransferDstOptimal,
                    VkImageLayout.TransferSrcOptimal,
                    i
                );
                command.TransferImageLayout(
                    obj.Image,
                    VkImageLayout.Undefined,
                    VkImageLayout.TransferDstOptimal,
                    i + 1
                );
                command.BlitImage(
                    obj.Image.ImageHandle,
                    0, 0, obj.Image.Width, obj.Image.Height,
                    i,
                    obj.Image.ImageHandle,
                    0, 0, obj.Image.Width, obj.Image.Height,
                    i + 1
                );
                command.TransferImageLayout(
                    obj.Image,
                    VkImageLayout.TransferSrcOptimal,
                    VkImageLayout.ShaderReadOnlyOptimal,
                    i
                );
            }
            command.TransferImageLayout(
                obj.Image,
                VkImageLayout.TransferDstOptimal,
                VkImageLayout.ShaderReadOnlyOptimal,
                obj.Image.MipLevel - 1
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

        internal BufferTransferObject UpdateUniformDataSemaphore<T>(string key, T data) where T : struct
        {
            return _descriptorMapper[key].Buffer.SetDataGetTransferObject(new T[] { data });
        }
    }
}