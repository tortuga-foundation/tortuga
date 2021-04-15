#pragma warning disable CS1591
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Linq;
using Tortuga.Graphics.API;
using System;
using Vulkan;

namespace Tortuga.Graphics
{
    public static partial class AssetLoader
    {
        public class JsonPipeline
        {
        }

        public class JsonBindings
        {
            public string Type { get; set; }
            public string Stage { get; set; }
            public JsonElement Data { get; set; }
        }

        public class JsonDescriptorSet
        {
            public string Name { get; set; }
            public List<JsonBindings> Bindings { get; set; }
        }

        public class JsonMaterial
        {
            public bool Instanced { get; set; }
            public JsonPipeline Pipeline { get; set; }
            public List<string> Shaders { get; set; }
            public List<JsonDescriptorSet> DescriptorSets { get; set; }
        }

        private static byte[] GetUniformBufferByteData(JsonElement data)
        {
            var type = data.GetProperty("Type").GetString();
            var rawValue = data.GetProperty("Value");
            if (type == nameof(Int32))
            {
                var val = JsonSerializer.Deserialize<List<Int32>>(rawValue.GetRawText());
                return val.ToArray().GetBytes();
            }
            else if (type == nameof(Single))
            {
                var val = JsonSerializer.Deserialize<List<Single>>(rawValue.GetRawText());
                return val.ToArray().GetBytes();
            }
            else
                throw new NotSupportedException("this type of data is currently not supported");
        }

        private static async Task<Texture> CombineTextureChannels(
            string r, string g, string b, string a
        )
        {
            Texture texture = null;
            // red
            if (r != null)
                texture = await AssetLoader.LoadTexture(r);

            // green
            if (g != null)
            {
                if (texture == null)
                    texture = await AssetLoader.LoadTexture(g);
                else
                    texture.CopyChannel(
                        await AssetLoader.LoadTexture(g),
                        TextureChannelFlags.G
                    );
            }

            // blue
            if (b != null)
            {
                if (texture == null)
                    texture = await AssetLoader.LoadTexture(b);
                else
                    texture.CopyChannel(
                        await AssetLoader.LoadTexture(b),
                        TextureChannelFlags.B
                    );
            }

            // alpha
            if (a != null)
            {
                if (texture == null)
                    texture = await AssetLoader.LoadTexture(a);
                else
                    texture.CopyChannel(
                        await AssetLoader.LoadTexture(a),
                        TextureChannelFlags.A
                    );
            }

            return texture;
        }

        private static async Task<Texture> GetCombinedImageSamplerImage(JsonElement data)
        {
            var type = data.GetProperty("Type").GetString();
            var rawValue = data.GetProperty("Value");
            if (type == "Texture")
            {
                return await AssetLoader.LoadTexture(rawValue.GetString());
            }
            else if (type == "TextureChannels")
            {
                string r = null, g = null, b = null, a = null;
                if (rawValue.TryGetProperty("R", out JsonElement rjson))
                    r = rjson.GetString();
                if (rawValue.TryGetProperty("G", out JsonElement gjson))
                    g = gjson.GetString();
                if (rawValue.TryGetProperty("B", out JsonElement bjson))
                    b = bjson.GetString();
                if (rawValue.TryGetProperty("A", out JsonElement ajson))
                    a = ajson.GetString();
                return await CombineTextureChannels(r, g, b, a);
            }
            else
                throw new NotSupportedException("this type of texture is currently not supported");
        }

        public static async Task<Material> LoadMaterial(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException();

            var content = await File.ReadAllTextAsync(file);
            var jsonMaterial = JsonSerializer.Deserialize<JsonMaterial>(
                content,
                new JsonSerializerOptions
                {
                    ReadCommentHandling = JsonCommentHandling.Skip
                }
            );
            var module = Engine.Instance.GetModule<GraphicsModule>();
            var device = module.GraphicsService.PrimaryDevice;

            var material = new Material(jsonMaterial);
            material.Instanced = jsonMaterial.Instanced;
            material.SetShaders(
                jsonMaterial.Shaders.Select(
                    s => new ShaderModule(device, s)
                ).ToList()
            );
            foreach (var jsonDescriptor in jsonMaterial.DescriptorSets)
            {
                // check if it is a pre-defined descriptor
                if (jsonDescriptor.Name.StartsWith('_'))
                {
                    material.InsertKey(jsonDescriptor.Name, null);
                    continue;
                }

                // setup descriptor layout
                material.InsertKey(
                    jsonDescriptor.Name,
                    new DescriptorLayout(
                        device,
                        jsonDescriptor.Bindings.Select(
                            (b, i) => new DescriptorBindingInfo(
                                (uint)i,
                                Enum.Parse<VkDescriptorType>(b.Type),
                                1,
                                Enum.Parse<VkShaderStageFlags>(b.Stage)
                            )
                        ).ToList()
                    )
                );

                // setup descriptors
                for (int i = 0; i < jsonDescriptor.Bindings.Count; i++)
                {
                    var binding = jsonDescriptor.Bindings[i];
                    var descriptorType = Enum.Parse<VkDescriptorType>(binding.Type);
                    if (descriptorType == VkDescriptorType.UniformBuffer)
                    {
                        var bytes = GetUniformBufferByteData(binding.Data);
                        material.BindBuffer(
                            jsonDescriptor.Name,
                            i,
                            bytes
                        );
                    }
                    else if (descriptorType == VkDescriptorType.CombinedImageSampler)
                    {
                        var image = await GetCombinedImageSamplerImage(binding.Data);
                        material.BindImage(
                            jsonDescriptor.Name,
                            i,
                            image
                        );
                    }
                    else
                        throw new NotSupportedException("this type of descriptor type binding is not supported");
                }
            }
            return material;
        }
    }
}