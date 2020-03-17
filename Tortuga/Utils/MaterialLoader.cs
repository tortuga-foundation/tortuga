using System.Collections.Generic;
using System.IO;
using System;
using Tortuga.Graphics;
using System.Numerics;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Tortuga.Utils
{
    public static class MaterialLoader
    {
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
                                images.Add(await ImageLoader.Load(binding.Image));
                            }
                            else if (binding.BuildImage != null)
                            {
                                var R = await ImageLoader.Load(binding.BuildImage["R"]);
                                if (binding.BuildImage.ContainsKey("G"))
                                    R.CopyChannel(await ImageLoader.Load(binding.BuildImage["G"]), Graphics.Image.Channel.G);
                                if (binding.BuildImage.ContainsKey("B"))
                                    R.CopyChannel(await ImageLoader.Load(binding.BuildImage["B"]), Graphics.Image.Channel.B);
                                if (binding.BuildImage.ContainsKey("A"))
                                    R.CopyChannel(await ImageLoader.Load(binding.BuildImage["A"]), Graphics.Image.Channel.A);
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
    }
}