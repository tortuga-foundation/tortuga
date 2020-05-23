using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Tortuga.Graphics.API;
using Vulkan;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Helps create and setup descriptor set objects
    /// </summary>
    public class DescritporSetHelper
    {
        /// <summary>
        /// If the pipeline needs to be re-compiled this will be set to true
        /// </summary>
        protected bool _isDirty = false;
        /// <summary>
        /// Descriptor set object
        /// </summary>
        internal struct DescriptorSetObject
        {
            /// <summary>
            /// Layout of the descriptor set
            /// </summary>
            public DescriptorSetLayout Layout;
            /// <summary>
            /// The descriptor set's pool
            /// </summary>
            public DescriptorSetPool Pool;
            /// <summary>
            /// descriptor set handle object
            /// </summary>
            public DescriptorSetPool.DescriptorSet Set;
            /// <summary>
            /// mapped buffers to this descriptor set
            /// </summary>
            public List<API.Buffer> Buffers;
            /// <summary>
            /// images used in the descriptor set
            /// </summary>
            public List<API.Image> Images;
            /// <summary>
            /// mapped image views in the descriptor set
            /// </summary>
            public List<ImageView> ImageViews;
            /// <summary>
            /// mapped samplers in the descriptor set
            /// </summary>
            public List<Sampler> Samplers;
        }

        internal DescriptorSetPool.DescriptorSet[] DescriptorSets
        {
            get
            {
                var sets = new List<DescriptorSetPool.DescriptorSet>();
                foreach (var obj in DescriptorMapper.Values)
                    sets.Add(obj.Set);
                return sets.ToArray();
            }
        }
        internal Dictionary<string, DescriptorSetObject> DescriptorMapper;

        /// <summary>
        /// Constructor for setting up a descriptor set helper object
        /// </summary>
        public DescritporSetHelper()
        {
            DescriptorMapper = new Dictionary<string, DescriptorSetObject>();
            _isDirty = true;
        }

        #region Uniform Buffer Descriptor Set

        /// <summary>
        /// Create a uniform buffer type descriptor set
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        /// <param name="byteSizes">for each binding in the descriptor set the size of the buffer</param>
        public void CreateUniformData(string key, uint[] byteSizes)
        {
            if (DescriptorMapper.ContainsKey(key))
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
            var buffers = new List<API.Buffer>();
            foreach (var byteSize in byteSizes)
            {
                buffers.Add(
                    API.Buffer.CreateDevice(
                        byteSize,
                        VkBufferUsageFlags.UniformBuffer
                    )
                );
            }
            set.BuffersUpdate(buffers.ToArray());

            DescriptorMapper.Add(
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
        /// <summary>
        /// Update an existing descriptor set with an array
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        /// <param name="binding">binding id for this descriptor set</param>
        /// <param name="data">the array data for the descriptor set</param>
        public async Task UpdateUniformDataArray<T>(string key, int binding, T[] data) where T : struct
        {
            if (DescriptorMapper.ContainsKey(key) == false)
                return;
            await DescriptorMapper[key].Buffers[binding].SetDataWithStaging(data);
        }
        /// <summary>
        /// Create a descriptor set with 1 binding. 
        /// The data is represented by the template you pass (must be struct)
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        public void CreateUniformData<A>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>())
            };
            CreateUniformData(key, sizes);
        }
        /// <summary>
        /// Create a descriptor set with 2 binding. 
        /// The data is represented by the template you pass (must be struct)
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        public void CreateUniformData<A, B>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<B>())
            };
            CreateUniformData(key, sizes);
        }
        /// <summary>
        /// Create a descriptor set with 3 binding. 
        /// The data is represented by the template you pass (must be struct)
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        public void CreateUniformData<A, B, C>(string key) where A : struct
        {
            var sizes = new uint[]{
                System.Convert.ToUInt32(Unsafe.SizeOf<A>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<B>()),
                System.Convert.ToUInt32(Unsafe.SizeOf<C>())
            };
            CreateUniformData(key, sizes);
        }
        /// <summary>
        /// Update a descriptor set with struct data
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        /// <param name="binding">binding if for this descriptor set</param>
        /// <param name="data">the data for the descriptor set</param>
        public async Task UpdateUniformData<T>(string key, int binding, T data) where T : struct
        {
            if (DescriptorMapper.ContainsKey(key) == false)
                return;
            await DescriptorMapper[key].Buffers[binding].SetDataWithStaging(new T[] { data });
        }

        /// <summary>
        /// Get uniform data from a descriptor set binding
        /// </summary>
        public async Task<T> GetUniformData<T>(string key, int binding) where T : struct
        {
            if (DescriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return (await DescriptorMapper[key].Buffers[binding].GetDataWithStaging<T>())[0];
        }
        /// <summary>
        /// Update a descriptor set uniform data async in rendering
        /// </summary>
        internal BufferTransferObject UpdateUniformDataSemaphore<T>(string key, int binding, T data) where T : struct
        {
            if (DescriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return DescriptorMapper[key].Buffers[binding].SetDataGetTransferObject(new T[] { data });
        }

        #endregion

        #region Sampled Image Descriptor Set

        /// <summary>
        /// Create a image type descriptor set
        /// </summary>
        /// <param name="key">the key representing this descriptor set</param>
        /// <param name="mipLevels">the amount of bindings with the mip levels for each binding</param>
        public void CreateSampledImage(string key, uint[] mipLevels)
        {
            if (DescriptorMapper.ContainsKey(key))
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
            DescriptorMapper.Add(key, new DescriptorSetObject
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
        /// <summary>
        /// Update a descriptor set of sampled image type
        /// </summary>
        /// <param name="key">Key representing the descriptor set</param>
        /// <param name="binding">the binding to update</param>
        /// <param name="image">image to update the descriptor set binding with</param>
        /// <returns>Task that will complete after the descriptor set has been updated</returns>
        public async Task UpdateSampledImage(string key, int binding, Image image)
        {
            if (DescriptorMapper.ContainsKey(key) == false)
                return;

            var obj = DescriptorMapper[key];
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
                DescriptorMapper[key] = obj;
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

            var staging = API.Buffer.CreateHost(
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

        #endregion
    }
}