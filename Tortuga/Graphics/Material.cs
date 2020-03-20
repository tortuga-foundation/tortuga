using Vulkan;
using Tortuga.Graphics.API;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Numerics;

namespace Tortuga.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public class Material
    {
        /// <summary>
        /// returns true if material uses instancing
        /// </summary>
        public bool IsInstanced => _isInstanced;

        private struct DescriptorSetObject
        {
            public DescriptorSetLayout Layout;
            public DescriptorSetPool Pool;
            public DescriptorSetPool.DescriptorSet Set;
            public List<API.Buffer> Buffers;
            public List<API.Image> Images;
            public List<ImageView> ImageViews;
            public List<Sampler> Samplers;
        }
        internal Pipeline ActivePipeline => _pipeline;
        internal API.Buffer InstanceBuffers => _instanceBuffer;
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

        private PipelineInputBuilder _inputBuilder;
        private Graphics.Shader _shader;
        private Pipeline _pipeline;
        private Dictionary<string, DescriptorSetObject> _descriptorMapper;
        private bool _isDirty;
        private API.Buffer _instanceBuffer;
        private bool _isInstanced = false;

        /// <summary>
        /// Constructor to create a basic material
        /// </summary>
        /// <param name="shader">Shader object used for this material</param>
        /// <param name="isInstanced">Does this material pass position, rotation and scale using instance buffer</param>
        public Material(
            Graphics.Shader shader,
            bool isInstanced = false
        )
        {

            _shader = shader;
            _descriptorMapper = new Dictionary<string, DescriptorSetObject>();
            _isInstanced = isInstanced;

            _inputBuilder = new PipelineInputBuilder();
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
                _inputBuilder.Bindings = new PipelineInputBuilder.BindingElement[]{
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
                _inputBuilder.Bindings = new PipelineInputBuilder.BindingElement[] { vertexBinding };

            _isDirty = true;
        }

        internal List<BufferTransferObject> BuildInstanceBuffers(Components.RenderMesh[] meshes)
        {
            var transferObjects = new List<BufferTransferObject>();
            var bytes = new List<byte>();
            foreach (var mesh in meshes)
            {
                foreach (var b in BitConverter.GetBytes(mesh.Position.X))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Position.Y))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Position.Z))
                    bytes.Add(b);

                foreach (var b in BitConverter.GetBytes(mesh.Rotation.X))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Rotation.Y))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Rotation.Z))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Rotation.W))
                    bytes.Add(b);


                foreach (var b in BitConverter.GetBytes(mesh.Scale.X))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Scale.Y))
                    bytes.Add(b);
                foreach (var b in BitConverter.GetBytes(mesh.Scale.Z))
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

        /// <summary>
        /// Creates a new pipeline object using the pipeline input builder and descriptor sets set by the user
        /// </summary>
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
                _inputBuilder.BindingDescriptions,
                _inputBuilder.AttributeDescriptions
            );
            _isDirty = false;
        }

        /// <summary>
        /// Update the material shader object
        /// </summary>
        /// <param name="shader">Shader object to use</param>
        public void UpdateShaders(Graphics.Shader shader)
        {
            _shader = shader;
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
        /// <summary>
        /// Update an existing descriptor set with an array
        /// </summary>
        /// <param name="key">key to represent this descriptor set</param>
        /// <param name="binding">binding id for this descriptor set</param>
        /// <param name="data">the array data for the descriptor set</param>
        public async Task UpdateUniformDataArray<T>(string key, int binding, T[] data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffers[binding].SetDataWithStaging(data);
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
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffers[binding].SetDataWithStaging(new T[] { data });
        }

        /// <summary>
        /// Get uniform data from a descriptor set binding
        /// </summary>
        public async Task<T> GetUniformData<T>(string key, int binding) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return (await _descriptorMapper[key].Buffers[binding].GetDataWithStaging<T>())[0];
        }
        /// <summary>
        /// Update a descriptor set uniform data async in rendering
        /// </summary>
        internal BufferTransferObject UpdateUniformDataSemaphore<T>(string key, int binding, T data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                throw new KeyNotFoundException();
            return _descriptorMapper[key].Buffers[binding].SetDataGetTransferObject(new T[] { data });
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
        /// <summary>
        /// Update a descriptor set of sampled image type
        /// </summary>
        /// <param name="key">Key representing the descriptor set</param>
        /// <param name="binding">the binding to update</param>
        /// <param name="image">image to update the descriptor set binding with</param>
        /// <returns>Task that will complete after the descriptor set has been updated</returns>
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

        #region Error Material

        /// <summary>
        /// Error material that is used in case there is an issue
        /// </summary>
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
        private static Material _cachedErrorMaterial;

        #endregion

        #region Material Loader
        private readonly static Dictionary<string, uint[]> _preDefinedUniforms = new Dictionary<string, uint[]>(){
            {
                "MODEL",
                new uint[]{ Convert.ToUInt32(Unsafe.SizeOf<Matrix4x4>()) }
            },
            {
                "LIGHT",
                new uint[]{ Convert.ToUInt32(Unsafe.SizeOf<Components.Light.FullShaderInfo>()) }
            }
        };

        private class ShaderJSON
        {
            public string Vertex { set; get; }
            public string Fragment { set; get; }
        }

        private class BindingValueJSON
        {
            public string Type { get; set; }
            public float Value { get; set; }
        }

        private class BindingsJSON
        {
            public IList<BindingValueJSON> Values { get; set; }
            public uint MipLevel { get; set; }
            public IDictionary<string, string> BuildImage { get; set; }
            public string Image { get; set; }
        }

        private class DescriptorSetJSON
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public IList<BindingsJSON> Bindings { get; set; }
        }

        private class MaterialJSON
        {
            public string Type { set; get; }
            public bool IsInstanced { set; get; }
            public ShaderJSON Shaders { get; set; }
            public IList<DescriptorSetJSON> DescriptorSets { get; set; }
        }

        /// <summary>
        /// Load a material json object into memory
        /// </summary>
        /// <param name="path">path to the material json object</param>
        /// <returns>material object that can be used for rendering</returns>
        public static async Task<Material> Load(string path)
        {
            if (File.Exists(path) == false)
                throw new FileNotFoundException("could not find materail file");

            var jsonContent = File.ReadAllText(path);
            try
            {
                var serializedData = JsonSerializer.Deserialize<MaterialJSON>(
                    jsonContent
                );
                var shader = Graphics.Shader.Load(
                    serializedData.Shaders.Vertex,
                    serializedData.Shaders.Fragment
                );
                var material = new Material(shader, serializedData.IsInstanced);
                foreach (var descriptorSet in serializedData.DescriptorSets)
                {
                    if (descriptorSet.Type == "UniformData")
                    {
                        if (_preDefinedUniforms.ContainsKey(descriptorSet.Name))
                        {
                            material.CreateUniformData(descriptorSet.Name, _preDefinedUniforms[descriptorSet.Name]);
                            continue;
                        }

                        var totalSize = new List<uint>();
                        var totalBytes = new List<byte[]>();
                        for (int i = 0; i < descriptorSet.Bindings.Count; i++)
                        {
                            var bytes = new List<byte>();
                            var binding = descriptorSet.Bindings[i];
                            foreach (var values in binding.Values)
                            {
                                if (values.Type == "Int")
                                {
                                    foreach (var b in BitConverter.GetBytes(Convert.ToInt32(values.Value)))
                                        bytes.Add(b);
                                }
                                else if (values.Type == "float")
                                {
                                    foreach (var b in BitConverter.GetBytes(values.Value))
                                        bytes.Add(b);
                                }
                            }
                            totalBytes.Add(bytes.ToArray());
                            totalSize.Add(Convert.ToUInt32(bytes.Count * sizeof(byte)));
                        }
                        material.CreateUniformData(descriptorSet.Name, totalSize.ToArray());
                        for (int i = 0; i < totalBytes.Count; i++)
                            await material.UpdateUniformDataArray(descriptorSet.Name, i, totalBytes[i]);
                    }
                    else if (descriptorSet.Type == "SampledImage2D")
                    {
                        var images = new List<Graphics.Image>();
                        var mipLevels = new List<uint>();
                        for (int i = 0; i < descriptorSet.Bindings.Count; i++)
                        {
                            var binding = descriptorSet.Bindings[i];
                            if (binding.Image != null)
                            {
                                mipLevels.Add(binding.MipLevel);
                                images.Add(await Graphics.Image.Load(binding.Image));
                            }
                            else if (binding.BuildImage != null)
                            {
                                var R = await Graphics.Image.Load(binding.BuildImage["R"]);
                                if (binding.BuildImage.ContainsKey("G"))
                                    R.CopyChannel(await Graphics.Image.Load(binding.BuildImage["G"]), Graphics.Image.Channel.G);
                                if (binding.BuildImage.ContainsKey("B"))
                                    R.CopyChannel(await Graphics.Image.Load(binding.BuildImage["B"]), Graphics.Image.Channel.B);
                                if (binding.BuildImage.ContainsKey("A"))
                                    R.CopyChannel(await Graphics.Image.Load(binding.BuildImage["A"]), Graphics.Image.Channel.A);
                                images.Add(R);
                                mipLevels.Add(binding.MipLevel);
                            }
                        }
                        material.CreateSampledImage(descriptorSet.Name, mipLevels.ToArray());
                        for (int i = 0; i < images.Count; i++)
                            await material.UpdateSampledImage(descriptorSet.Name, i, images[i]);
                    }
                }
                return material;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
            return Material.ErrorMaterial;
        }
        #endregion
    }
}