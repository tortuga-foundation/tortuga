using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Drawing;

namespace Tortuga.Graphics
{
    public class Material
    {
        public Matrix4x4 Model;

        internal Shader Vertex => _vertex;
        internal Shader Fragment => _fragment;
        internal Pipeline ActivePipeline => _pipeline;
        internal List<DescriptorSetPool.DescriptorSet> DescriptorSets => _descriptorSets;
        internal bool UsingLighting => _usingLighting;

        private Shader _vertex;
        private Shader _fragment;
        private Pipeline _pipeline;
        private List<DescriptorSetLayout> _layouts;
        private List<DescriptorSetPool> _setPool;
        private List<DescriptorSetPool.DescriptorSet> _descriptorSets;
        private List<Buffer> _setBuffers;
        private List<Tortuga.Graphics.API.Image> _setImages;
        private List<ImageView> _setImageViews;
        private List<Sampler> _setSamplers;
        private bool _isDirty;
        private bool _usingLighting;

        public Material(string vertexShader, string fragmentShader, bool includeLighting = true)
        {
            _usingLighting = includeLighting;
            _vertex = new Shader(vertexShader);
            _fragment = new Shader(fragmentShader);

            _layouts = new List<DescriptorSetLayout>();
            _setPool = new List<DescriptorSetPool>();
            _descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            _setBuffers = new List<Buffer>();
            _setImages = new List<API.Image>();
            _setImageViews = new List<ImageView>();
            _setSamplers = new List<Sampler>();

            //model matrix
            AddBuffersToDescriptorSets<Matrix4x4>(new DescriptorSetCreateInfo[]{
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.All,
                    type = VkDescriptorType.UniformBuffer
                }
            });

            if (_usingLighting)
            {
                //lighting
                AddBuffersToDescriptorSets<Systems.RenderingSystem.LightShaderInfo>(new DescriptorSetCreateInfo[]{
                    new DescriptorSetCreateInfo
                    {
                        stage = VkShaderStageFlags.All,
                        type = VkDescriptorType.UniformBuffer
                    }
                });
            }
        }

        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var totalDescriptorSets = new List<DescriptorSetLayout>();
            totalDescriptorSets.Add(Engine.Instance.CameraDescriptorLayout);
            foreach (var l in _layouts)
                totalDescriptorSets.Add(l);

            _pipeline = new Pipeline(
                totalDescriptorSets.ToArray(),
                _vertex,
                _fragment
            );
            _isDirty = false;
        }

        internal BufferTransferObject ModelTransferObject(Matrix4x4 model)
        {
            return _setBuffers[0].SetDataGetTransferObject(new Matrix4x4[] { model });
        }
        internal BufferTransferObject LightingTransferObject(Systems.RenderingSystem.LightShaderInfo info)
        {
            if (_usingLighting == false)
                throw new System.Exception("Lighting is disabled for this material but it is still being used in rendering");
            return _setBuffers[1].SetDataGetTransferObject(new Systems.RenderingSystem.LightShaderInfo[] { info });
        }

        public void UpdateShaders(string vertex, string fragment)
        {
            _vertex = new Shader(vertex);
            _fragment = new Shader(fragment);
            _isDirty = true;
        }

        private List<int> AddBuffersToDescriptorSets<T>(DescriptorSetCreateInfo[] createInfo)
        {
            var layout = new DescriptorSetLayout(createInfo);
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var buffers = new List<Buffer>();
            var rtn = new List<int>();
            foreach (var info in createInfo)
            {
                if (info.type != VkDescriptorType.UniformBuffer)
                    throw new System.NotSupportedException("only uniform buffers are supported by this method");

                var buffer = Buffer.CreateDevice(
                    System.Convert.ToUInt32(Unsafe.SizeOf<T>()),
                    VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst
                );
                buffers.Add(buffer);
                _setBuffers.Add(buffer);
                rtn.Add(_setBuffers.Count - 1);
            }
            set.BuffersUpdate(buffers.ToArray());
            _layouts.Add(layout);
            _setPool.Add(pool);
            _descriptorSets.Add(set);
            _isDirty = true;
            return rtn;
        }

        private List<int> AddImageToDescriptorSets(DescriptorSetCreateInfo[] createInfo, uint imageWidth, uint imageHeight)
        {
            var rtn = new List<int>();
            var layout = new DescriptorSetLayout(createInfo);
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var imageViews = new List<ImageView>();
            var samplers = new List<Sampler>();
            foreach (var info in createInfo)
            {
                if (info.type != VkDescriptorType.SampledImage)
                    throw new System.NotSupportedException("only sampled images are supported by this method");

                var image = new Tortuga.Graphics.API.Image(
                    imageWidth,
                    imageHeight,
                    VkFormat.R8g8b8a8Srgb,
                    VkImageUsageFlags.Sampled | VkImageUsageFlags.TransferDst,
                    1
                );
                var imageView = new ImageView(
                    image,
                    VkImageAspectFlags.Color
                );
                var sampler = new Sampler();
                _setImages.Add(image);
                _setImageViews.Add(imageView);
                _setSamplers.Add(sampler);
                imageViews.Add(imageView);
                samplers.Add(sampler);
                rtn.Add(_setImages.Count - 1);
            }
            set.SampledImageUpdate(imageViews.ToArray(), samplers.ToArray());
            _layouts.Add(layout);
            _setPool.Add(pool);
            _descriptorSets.Add(set);
            _isDirty = true;
            return rtn;
        }

        public int[] CreateUniformData<T>(int bindings = 1)
        {
            var info = new DescriptorSetCreateInfo[bindings];
            for (int i = 0; i < info.Length; i++)
                info[i] = new DescriptorSetCreateInfo
                {
                    type = VkDescriptorType.UniformBuffer,
                    stage = VkShaderStageFlags.All
                };
            return AddBuffersToDescriptorSets<T>(info).ToArray();
        }
        public async Task UpdateUniformData<T>(int i, T[] data) where T : struct
            => await _setBuffers[i].SetDataWithStaging<T>(data);

        private struct VulkanPixel
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;
        };

        public int[] CreateSampledImage(uint width, uint height, int bindings = 1)
        {
            var info = new DescriptorSetCreateInfo[bindings];
            for (int i = 0; i < info.Length; i++)
                info[i] = new DescriptorSetCreateInfo
                {
                    type = VkDescriptorType.SampledImage,
                    stage = VkShaderStageFlags.Fragment
                };
            return AddImageToDescriptorSets(info, width, height).ToArray();
        }
        public Task UpdateSampledImage(int i, Color[] pixels)
        {
            if (pixels.Length != _setImages[i].Width * _setImages[i].Height)
                throw new System.Exception("pixels don't match image size");

            return Task.Run(() =>
            {
                //create staging buffer for image
                var vulkanPixels = new VulkanPixel[pixels.Length];
                for (int j = 0; j < pixels.Length; j++)
                {
                    vulkanPixels[j].R = pixels[j].R;
                    vulkanPixels[j].G = pixels[j].G;
                    vulkanPixels[j].B = pixels[j].B;
                    vulkanPixels[j].A = pixels[j].A;
                }
                var buffer = Buffer.CreateHost(
                    System.Convert.ToUInt32(Unsafe.SizeOf<VulkanPixel>() * vulkanPixels.Length),
                    VkBufferUsageFlags.TransferSrc
                );
                buffer.SetData(vulkanPixels);

                var fence = new Fence();
                var commandPool = new CommandPool(
                    Engine.Instance.MainDevice.GraphicsQueueFamily
                );
                var command = commandPool.AllocateCommands()[0];
                command.Begin(VkCommandBufferUsageFlags.OneTimeSubmit);
                command.TransferImageLayout(_setImages[i], VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal);
                command.BufferToImage(buffer, _setImages[i]);
                command.TransferImageLayout(_setImages[i], VkImageLayout.TransferDstOptimal, VkImageLayout.ShaderReadOnlyOptimal);
                command.End();
                command.Submit(
                    Engine.Instance.MainDevice.GraphicsQueueFamily.Queues[0],
                    null, null,
                    fence
                );
                fence.Wait();
            });
        }
    }
}