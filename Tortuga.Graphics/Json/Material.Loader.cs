using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using Tortuga.Core.Json;
using Tortuga.Graphics.API;
using static Tortuga.Graphics.Texture;

namespace Tortuga.Graphics.Json
{
    /// <summary>
    /// json shader type
    /// </summary>
    public class JsonShader
    {
        /// <summary>
        /// path to vertex shader
        /// </summary>
        public string VertexPath { get; set; }
        /// <summary>
        /// path to fragment shader
        /// </summary>
        public string FragmentPath { get; set; }
        /// <summary>
        /// vertex shader content
        /// </summary>
        public string Vertex { get; set; }
        /// <summary>
        /// fragment shader content
        /// </summary>
        public string Fragment { get; set; }
    }

    /// <summary>
    /// json descriptor set binding type
    /// </summary>
    public class JsonBinding
    {
        /// <summary>
        /// what stage to assing this binding to
        /// </summary>
        public string Stage { get; set; }
        /// <summary>
        /// what type of descriptor is this binding referring to
        /// </summary>
        public string DescriptorType { get; set; }
        /// <summary>
        /// value of the descriptor set binding
        /// </summary>
        public JsonElement Value { get; set; }
    }

    /// <summary>
    /// json descriptor set type
    /// </summary>
    public class JsonDescriptorSet
    {
        /// <summary>
        /// Name of the descriptor set, some of these might be reserved
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// list of bindings for descriptor set
        /// </summary>
        public ICollection<JsonBinding> Bindings { get; set; }
    }

    /// <summary>
    /// json material type
    /// </summary>
    public class JsonMaterial
    {
        /// <summary>
        /// shader information
        /// </summary>
        public JsonShader Shaders { get; set; }
        /// <summary>
        /// list of descriptor sets
        /// </summary>
        public ICollection<JsonDescriptorSet> DescriptorSets { get; set; }
    }

    /// <summary>
    /// json material mapper
    /// </summary>
    public static class JsonMaterialMapper
    {
        private static Task<Texture> UpdateTexture(
            Texture texture,
            Channel channel,
            Vector4 color
        )
         => Task.Run(() =>
        {
            if (texture == null)
            {
                return Texture.SingleColor(
                    Color.FromArgb(
                        Convert.ToInt32(color.W),
                        Convert.ToInt32(color.X),
                        Convert.ToInt32(color.Y),
                        Convert.ToInt32(color.Z)
                    )
                );
            }
            else
            {
                texture.CopyChannel(
                    Texture.SingleColor(
                        Color.FromArgb(
                            Convert.ToInt32(color.W),
                            Convert.ToInt32(color.X),
                            Convert.ToInt32(color.Y),
                            Convert.ToInt32(color.Z)
                        )
                    ),
                    channel
                );
                return texture;
            }
        });

        private static async Task<Texture> UpdateTexture(
            Texture texture,
            Channel channel,
            string filePath)
        {
            if (texture == null)
            {
                return await Texture.Load(filePath);
            }
            else
            {
                texture.CopyChannel(
                   await Texture.Load(filePath),
                   channel
               );
                return texture;
            }
        }

        private static async Task<Texture> UpdateTexture(
            Texture texture,
            Channel channel,
            object data,
            Type dataType
        )
        {
            if (dataType.Equals(typeof(string)))
                return await UpdateTexture(texture, channel, (string)data);
            else if (dataType.Equals(typeof(Vector4)))
                return await UpdateTexture(texture, channel, (Vector4)data);
            else
                throw new Exception("unknown type was passed");
        }

        private static async Task SetupMaterialDescriptorSet(
            Material mat,
            DescriptorType type,
            JsonDescriptorSet descriptor,
            int i,
            Type objectType,
            object rawData
        )
        {
            if (
                type == DescriptorType.StorageBuffer ||
                type == DescriptorType.StorageBufferDynamic ||
                type == DescriptorType.StorageTexelBuffer ||
                type == DescriptorType.UniformBuffer ||
                type == DescriptorType.UniformBufferDynamic ||
                type == DescriptorType.UniformTexelBuffer
            )
            {
                await mat.BindBuffer(
                    descriptor.Name,
                    i,
                    rawData.GetBytes(objectType)
                );
            }
            else if (
                type == DescriptorType.CombinedImageSampler ||
                type == DescriptorType.SampledImage ||
                type == DescriptorType.StorageImage
            )
            {
                Texture image = null;
                if (objectType.Equals(typeof(string)))
                {
                    image = await Texture.Load(((string)rawData));
                }
                else if (objectType.Equals(typeof(JsonImageChannel)))
                {
                    var data = (JsonImageChannel)rawData;
                    if (data.R.ToObject(out Type objectTypeR, out object dataR))
                        image = await UpdateTexture(image, Channel.R, dataR, objectTypeR);
                    if (data.G.ToObject(out Type objectTypeG, out object dataG))
                        image = await UpdateTexture(image, Channel.G, dataG, objectTypeG);
                    if (data.B.ToObject(out Type objectTypeB, out object dataB))
                        image = await UpdateTexture(image, Channel.B, dataB, objectTypeB);
                    if (data.A.ToObject(out Type objectTypeA, out object dataA))
                        image = await UpdateTexture(image, Channel.A, dataA, objectTypeA);
                }

                await mat.BindImage(
                    descriptor.Name,
                    i,
                    image.Pixels,
                    image.Width,
                    image.Height
                );
            }
        }

        /// <summary>
        /// json material to tortuga material converter
        /// </summary>
        /// /// <param name="material">json material</param>
        /// <returns>tortuga material object</returns>
        public static async Task<Material> ToMaterial(this JsonMaterial material)
        {
            var graphicsModule = Engine.Instance.GetModule<GraphicsModule>();
            var mat = new Material(
                new API.Shader(material.Shaders.VertexPath),
                new API.Shader(material.Shaders.FragmentPath)
            );
            foreach (var descriptor in material.DescriptorSets)
            {
                var bindings = descriptor.Bindings.ToList();

                //setup descriptor type create info
                var bindingsCreateInfos = new List<API.DescriptorSetCreateInfo>();
                foreach (var binding in bindings)
                {
                    bindingsCreateInfos.Add(
                        API.DescriptorSetCreateInfo.TryParse(
                            binding.DescriptorType,
                            binding.Stage
                        )
                    );
                }

                //setup descriptor type
                mat.InsertKey(
                    descriptor.Name,
                    new API.DescriptorSetLayout(
                        API.Handler.MainDevice,
                        bindingsCreateInfos.ToArray()
                    )
                );

                //update descriptor set
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.Value.ToObject(
                        out System.Type objectType,
                        out object rawData
                    ) == false)
                        throw new Exception("invalid json was passed");

                    var bindingCreateInfo = bindingsCreateInfos[i];
                    await SetupMaterialDescriptorSet(
                        mat,
                        bindingCreateInfo.type,
                        descriptor,
                        i,
                        objectType,
                        rawData
                    );


                    //     else if (objectType.Equals(typeof(float[])))
                    //     {
                    //         var data = (float[])rawData;
                    //         await mat.BindBuffer(
                    //             descriptor.Name,
                    //             i,
                    //             data
                    //         );
                    //     }
                    //     else if (objectType.Equals(typeof(string)))
                    //     {
                    //         if (bindingCreateInfo.type == API.DescriptorType.CombinedImageSampler)
                    //         {

                    //         }
                    //     }
                    //     await mat.BindBuffer(
                    //         descriptor.Name,
                    //         i,
                    //         new byte[] { data }
                    //     );

                    //     var type = binding.Value.GetProperty("Type").GetString();
                    //     switch (type)
                    //     {
                    //         case "Int32":
                    //             {
                    //                 var data = binding.Value.GetProperty("Data").GetInt32();
                    //                 await mat.BindBuffer(
                    //                     descriptor.Name,
                    //                     i,
                    //                     new int[] { data }
                    //                 );
                    //             }
                    //             break;
                    //         case "Float":
                    //             {

                    //             }
                    //             break;
                    //         case "Image":
                    //             {
                    //                 var data = binding.Value.GetProperty("Data").GetString();
                    //                 var texture = await Texture.Load(data);
                    //                 await mat.BindImage(
                    //                     descriptor.Name,
                    //                     i,
                    //                     texture.Pixels,
                    //                     texture.Width,
                    //                     texture.Height
                    //                 );
                    //             }
                    //             break;
                    //         case "ImageChannels":
                    //             {
                    //                 var data = binding.Value.GetProperty("Data").ToString();
                    //                 var imageChannels = JsonSerializer.Deserialize<JsonChannelsData>(data);
                    //                 var R = await Texture.Load(imageChannels.R);
                    //                 var G = await Texture.Load(imageChannels.G);
                    //                 var B = await Texture.Load(imageChannels.B);
                    //                 R.CopyChannel(G, Texture.Channel.G);
                    //                 R.CopyChannel(B, Texture.Channel.B);
                    //                 await mat.BindImage(
                    //                     descriptor.Name,
                    //                     i,
                    //                     R.Pixels,
                    //                     R.Width,
                    //                     R.Height
                    //                 );
                    //             }
                    //             break;
                    //     }
                }
            }
            mat.ReCompilePipeline();
            return mat;
        }
    }
}