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
    public class Material : DescritporSetHelper
    {
        /// <summary>
        /// returns true if material uses instancing
        /// </summary>
        public bool IsInstanced => _isInstanced;

        internal Pipeline ActivePipeline => _pipeline;
        internal API.Buffer InstanceBuffers => _instanceBuffer;

        private PipelineInputBuilder _inputBuilder;
        private Graphics.Shader _shader;
        private Pipeline _pipeline;
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

        /// <summary>
        /// Constructor to create a basic material
        /// </summary>
        /// <param name="shader">Shader object used for this material</param>
        /// <param name="pipelineInputBuilder">The pipeline input for this material</param>
        public Material(
            Graphics.Shader shader,
            PipelineInputBuilder pipelineInputBuilder
        )
        {
            _shader = shader;
            _inputBuilder = pipelineInputBuilder;
            _isInstanced = false;
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
        public virtual void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var totalDescriptorSets = new List<DescriptorSetLayout>();
            totalDescriptorSets.Add(Engine.Instance.CameraDescriptorLayout);
            totalDescriptorSets.Add(Engine.Instance.ModelDescriptorLayout);
            foreach (var l in this.DescriptorMapper.Values)
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
            this._isDirty = true;
        }


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