using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
    public class JsonBinding : Core.JsonBaseType
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
    public class JsonDescriptorSet : Core.JsonBaseType
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
    public class JsonMaterial : Core.JsonBaseType
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
        /// <summary>
        /// json material to tortuga material converter
        /// </summary>
        /// <param name="material">json material</param>
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
                var bindingsCreateInfo = new List<API.DescriptorSetCreateInfo>();
                foreach (var binding in bindings)
                {
                    bindingsCreateInfo.Add(
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
                        bindingsCreateInfo.ToArray()
                    )
                );

                //update descriptor set
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    var type = binding.Value.GetProperty("Type").GetString();
                    switch (type)
                    {
                        case "Int32":
                            {
                                var data = binding.Value.GetProperty("Data").GetInt32();
                                await mat.BindBuffer(
                                    descriptor.Name,
                                    i,
                                    new int[] { data }
                                );
                            }
                            break;
                        case "Image":
                            {
                                var data = binding.Value.GetProperty("Data").GetString();
                                var texture = await Texture.Load(data);
                                await mat.BindImage(
                                    descriptor.Name,
                                    i,
                                    texture.Pixels,
                                    texture.Width,
                                    texture.Height
                                );
                            }
                            break;
                        case "ImageChannels":
                            {
                                var data = binding.Value.GetProperty("Data").ToString();
                                var imageChannels = JsonSerializer.Deserialize<JsonChannelsData>(data);
                                var R = await Texture.Load(imageChannels.R);
                                var G = await Texture.Load(imageChannels.G);
                                var B = await Texture.Load(imageChannels.B);
                                R.CopyChannel(G, Texture.Channel.G);
                                R.CopyChannel(B, Texture.Channel.B);
                                await mat.BindImage(
                                    descriptor.Name,
                                    i,
                                    R.Pixels,
                                    R.Width,
                                    R.Height
                                );
                            }
                            break;
                    }
                }
            }
            mat.ReCompilePipeline();
            return mat;
        }
    }
}