using System;
using Tortuga.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Tortuga.Graphics.API;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Numerics;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Responsible for mesh object io
    /// </summary>
    public static class MaterialLoader
    {
        private class Loaders
        {
            public class TexturePath { }

            public class TexturePathChannels
            {
                public string R { get; set; }
                public string G { get; set; }
                public string B { get; set; }
                public string A { get; set; }
            }

            public class ShaderFile
            {
                public string ShaderType { get; set; }
                public string Data { get; set; }
            }

            public class DescriptorSetBinding
            {
                public string DescriptorType { get; set; }
                public string Stage { get; set; }
                public object Data { get; set; }
            }

            public class DescriptorSet
            {
                public string Name { get; set; }
                public List<DescriptorSetBinding> Bindings { get; set; }
            }

            public class Material
            {
                public List<ShaderFile> Shaders { get; set; }
                public List<DescriptorSet> DescriptorSets { get; set; }
            }
        }

        private class BindingHelper
        {
            public Vulkan.VkDescriptorType Type;
            public Texture Image;
            public byte[] Data;
        }

        private static bool TextureLoaderHelper(
            Texture texture,
            string path,
            TextureChannelFlags channel,
            bool isInit
        )
        {
            if (string.IsNullOrEmpty(path) == false)
            {
                if (isInit == false)
                {
                    texture.Load(path).Wait();
                }
                else
                {
                    var R = new Texture();
                    R.Load(path).Wait();
                    texture.CopyChannel(R, TextureChannelFlags.R);
                }
            }
            return isInit;
        }

        internal static void Init()
        {
            JsonUtility.DefaultProperties.Add(
                typeof(Loaders.TexturePath).Name,
                (Type t, JsonElement el) =>
                {
                    var texture = new Texture();
                    texture.Load(el.GetString()).Wait();
                    return texture;
                }
            );
            JsonUtility.DefaultProperties.Add(
                typeof(Loaders.TexturePathChannels).Name,
                (Type t, JsonElement el) =>
                {
                    var texture = new Texture();
                    var texturePaths = JsonSerializer.Deserialize<Loaders.TexturePathChannels>(el.GetRawText());
                    bool isTextureInit = false;
                    isTextureInit = TextureLoaderHelper(
                        texture,
                        texturePaths.R,
                        TextureChannelFlags.R,
                        isTextureInit
                    );
                    isTextureInit = TextureLoaderHelper(
                        texture,
                        texturePaths.G,
                        TextureChannelFlags.G,
                        isTextureInit
                    );
                    isTextureInit = TextureLoaderHelper(
                        texture,
                        texturePaths.B,
                        TextureChannelFlags.B,
                        isTextureInit
                    );
                    isTextureInit = TextureLoaderHelper(
                        texture,
                        texturePaths.A,
                        TextureChannelFlags.A,
                        isTextureInit
                    );
                    return texture;
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="material"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task Load(this Material material, string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            var content = await File.ReadAllTextAsync(file);
            var jsonMaterial = JsonUtility.JsonToDataType<Loaders.Material>(content);
            var device = material.Device;

            //clear all bound images and buffers from material
            material.Clear();

            // setup shaders
            var shaderPaths = new List<string>();
            foreach (var shaderPath in jsonMaterial.Shaders)
                shaderPaths.Add(shaderPath.Data);
            material.SetShaders(shaderPaths);

            //setup descriptor sets
            foreach (var descriptorSet in jsonMaterial.DescriptorSets)
            {
                var bindingDataHelper = new List<BindingHelper>();
                //construct descriptor layouts
                var bindingInfo = new List<DescriptorBindingInfo>();
                for (int i = 0; i < descriptorSet.Bindings.Count; i++)
                {
                    var binding = descriptorSet.Bindings[i];
                    var descriptorType = Enum.Parse<Vulkan.VkDescriptorType>(binding.DescriptorType);
                    var shaderStage = Enum.Parse<Vulkan.VkShaderStageFlags>(binding.Stage);
                    bindingInfo.Add(new DescriptorBindingInfo
                    {
                        DescriptorCounts = 1,
                        DescriptorType = descriptorType,
                        Index = Convert.ToUInt32(i),
                        ShaderStageFlags = shaderStage
                    });

                    var bindingData = (KeyValuePair<string, object>)binding.Data;

                    if (descriptorType == Vulkan.VkDescriptorType.UniformBuffer)
                    {
                        var byteArray = new byte[] { };
                        if (bindingData.Key == "Int32")
                            byteArray = (bindingData.Value as List<int>).ToArray().GetBytes();
                        else if (bindingData.Key == "Float" || bindingData.Key == "float")
                            byteArray = (bindingData.Value as List<float>).ToArray().GetBytes();
                        else if (bindingData.Key == "Vector2")
                            byteArray = ((Vector2)bindingData.Value).GetBytes();
                        else if (bindingData.Key == "Vector3")
                            byteArray = ((Vector3)bindingData.Value).GetBytes();
                        else if (bindingData.Key == "Vector4")
                            byteArray = ((Vector4)bindingData.Value).GetBytes();
                        else if (bindingData.Key == "Matrix4x4")
                            byteArray = ((Matrix4x4)bindingData.Value).GetBytes();
                        else
                            throw new NotSupportedException("This data type is not supported");

                        bindingDataHelper.Add(new BindingHelper
                        {
                            Type = descriptorType,
                            Image = null,
                            Data = byteArray
                        });
                    }
                    else if (descriptorType == Vulkan.VkDescriptorType.CombinedImageSampler)
                    {
                        bindingDataHelper.Add(new BindingHelper
                        {
                            Type = descriptorType,
                            Image = bindingData.Value as Texture,
                            Data = null
                        });
                    }
                    else
                        throw new NotSupportedException("Only `UniformBuffer` and `CombinedImageSampler` are currently supported");
                }
                material.InsertKey(
                    descriptorSet.Name,
                    new DescriptorLayout(
                        device,
                        bindingInfo
                    )
                );

                //set descriptor values
                for (int i = 0; i < bindingDataHelper.Count; i++)
                {
                    var binding = bindingDataHelper[i];
                    if (binding.Type == Vulkan.VkDescriptorType.UniformBuffer)
                    {
                        material.BindBuffer(
                            descriptorSet.Name,
                            i,
                            binding.Data
                        );
                    }
                    else if (binding.Type == Vulkan.VkDescriptorType.CombinedImageSampler)
                    {
                        material.BindImage(
                            descriptorSet.Name,
                            i,
                            binding.Image
                        );
                    }
                }
            }
        }
    }
}