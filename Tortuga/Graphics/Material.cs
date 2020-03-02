using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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

        public void CreateUniformData(string key, uint byteSize)
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
                byteSize,
                VkBufferUsageFlags.UniformBuffer
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
        public async Task UpdateUniformDataArray<T>(string key, T[] data) where T : struct
        {
            if (_descriptorMapper.ContainsKey(key) == false)
                return;
            await _descriptorMapper[key].Buffer.SetDataWithStaging(data);
        }
        public void CreateUniformData<T>(string key) where T : struct
        {
            CreateUniformData(
                key,
                System.Convert.ToUInt32(Unsafe.SizeOf<T>())
            );
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
        public async Task<T> GetUniformData<T>(string key) where T : struct
        {
            return (await _descriptorMapper[key].Buffer.GetDataWithStaging<T>())[0];
        }

        internal BufferTransferObject UpdateUniformDataSemaphore<T>(string key, T data) where T : struct
        {
            return _descriptorMapper[key].Buffer.SetDataGetTransferObject(new T[] { data });
        }

        public static Material Load(string path)
        {
            if (File.Exists(path) == false)
                throw new FileNotFoundException("could not find materail file");

            var obj = Json.JsonParser.FromJson(File.ReadAllText(path));
            try
            {
                var lighting = (bool)obj["Light"];
                //setup shader
                var shadersJSON = obj["Shaders"] as Dictionary<string, object>;
                var vertexShader = shadersJSON["Vertex"] as string;
                var fragmentShader = shadersJSON["Fragment"] as string;
                var shader = Shader.Load(vertexShader, fragmentShader);

                //create new material
                var material = new Material(shader, lighting);
                //setup material descriptor sets
                var setsJSON = obj["DescriptorSets"] as ICollection<object>;
                foreach (var setRawJSON in setsJSON)
                {
                    var setJSON = setRawJSON as Dictionary<string, object>;
                    var setType = setJSON["Type"] as string;
                    var setName = setJSON["Name"] as string;

                    if (setType == "UniformData")
                    {
                        var setValue = setJSON["value"] as Dictionary<string, object>;
                        var data = new List<byte>();
                        foreach (var item in setValue)
                        {
                            var raw = item.Value as Dictionary<string, object>;
                            var type = raw["Type"] as string;
                            if (type == "Int")
                            {
                                var val = System.Convert.ToInt32(
                                    System.Math.Round((double)raw["Value"])
                                );
                                var bytes = System.BitConverter.GetBytes(val);
                                foreach (var b in bytes)
                                    data.Add(b);
                            }
                            else if (type == "Double")
                            {
                                var val = (double)raw["Value"];
                                var bytes = System.BitConverter.GetBytes(val);
                                foreach (var b in bytes)
                                    data.Add(b);
                            }
                            else if (type == "Float")
                            {
                                var val = System.Convert.ToSingle((double)raw["Value"]);
                                var bytes = System.BitConverter.GetBytes(val);
                                foreach (var b in bytes)
                                    data.Add(b);
                            }
                            else if (type == "Vec3")
                            {
                                var val = raw["Value"] as string;
                                var reg = new Regex(@"[a-zA-Z0-9]{4}[\ ]*\([\ ]*([0-9\,\-]+)[\ ]*,[\ ]*([0-9\,\-]+)[\ ]*,[\ ]*([0-9\,\-]+)[\ ]*\)");
                                var match = reg.Match(val);
                                var axies = new List<float>();
                                axies.Add(float.Parse(match.Groups[1].ToString()));
                                axies.Add(float.Parse(match.Groups[2].ToString()));
                                axies.Add(float.Parse(match.Groups[3].ToString()));
                                foreach (var ax in axies)
                                {
                                    var bytes = System.BitConverter.GetBytes(ax);
                                    foreach (var b in bytes)
                                        data.Add(b);
                                }
                            }
                        }
                        material.CreateUniformData(
                            setName,
                            System.Convert.ToUInt32(data.Count() * sizeof(byte))
                        );
                        material.UpdateUniformDataArray<byte>(setName, data.ToArray()).Wait();
                    }
                    else if (setType == "SampledImage2D")
                    {
                        var mipLevel = System.Convert.ToInt32(System.Math.Round((double)setJSON["MipLevel"]));
                        material.CreateSampledImage(setName, 1, 1);
                        var singleImage = setJSON["Value"] as string;
                        if (singleImage != null)
                        {
                            if (File.Exists(singleImage))
                                material.UpdateSampledImage(setName, new Graphics.Image(singleImage)).Wait();
                        }
                        else
                        {
                            var rawMultiImage = setJSON["Value"] as ICollection<object>;
                            var multiImage = rawMultiImage.ToArray();
                            if (multiImage.Length <= 4 && multiImage.Length > 0)
                            {
                                var R = new Graphics.Image(multiImage[0] as string);
                                if (multiImage.Length > 1)
                                {
                                    var G = new Graphics.Image(multiImage[1] as string);
                                    R.CopyChannel(G, Graphics.Image.Channel.G);
                                }
                                if (multiImage.Length > 2)
                                {
                                    var B = new Graphics.Image(multiImage[2] as string);
                                    R.CopyChannel(B, Graphics.Image.Channel.B);
                                }
                                if (multiImage.Length > 3)
                                {
                                    var A = new Graphics.Image(multiImage[3] as string);
                                    R.CopyChannel(A, Graphics.Image.Channel.A);
                                }
                                material.UpdateSampledImage(setName, R).Wait();
                            }
                        }
                    }
                }
                return material;
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
            return ErrorMaterial;
        }

        public static Material ErrorMaterial
        {
            get
            {
                var material = new Material(
                    Graphics.Shader.Load(
                        "Assets/Shaders/Error/Error.vert",
                        "Assets/Shaders/Error/Error.frag"
                    )
                );
                return material;
            }
        }
    }
}