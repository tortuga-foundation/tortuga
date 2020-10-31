#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Text.Json;
using Tortuga.Core.Json;
using Tortuga.Graphics.Json;

namespace Tortuga.Graphics
{
    /// <summary>
    /// Graphics module
    /// </summary>
    public class GraphicsModule : Core.BaseModule
    {
        //MRT renderer
        internal API.DescriptorSetLayout[] MeshDescriptorSetLayouts => _meshDescriptorSetLayouts;
        private API.DescriptorSetLayout[] _meshDescriptorSetLayouts;
        internal API.RenderPass MeshRenderPassMRT => _meshRenderPassMRT;
        private API.RenderPass _meshRenderPassMRT;

        //deffered
        internal API.DescriptorSetLayout[] DefferedDescriptorSetLayouts => _defferedDescriptorSetLayouts;
        private API.DescriptorSetLayout[] _defferedDescriptorSetLayouts;
        internal API.RenderPass DefferedRenderPass => _defferedRenderPass;
        private API.RenderPass _defferedRenderPass;

        //light
        internal API.DescriptorSetLayout[] LightDescriptorSetLayouts => _lightDescriptorSetLayouts;
        private API.DescriptorSetLayout[] _lightDescriptorSetLayouts;
        internal API.RenderPass LightRenderPass => _lightRenderPass;
        private API.RenderPass _lightRenderPass;

        public override void Destroy()
        {
        }

        public override void Init()
        {
            //initialize vulkan
            API.Handler.Init();

            #region Setup Json Data Types

            JsonUtility.DataConverter.Add(
                "String",
                new KeyValuePair<Type, Func<JsonElement, object>>(
                    typeof(string),
                    (JsonElement el) => el.GetString()
                )
            );

            JsonUtility.DataConverter.Add(
                "Channels",
                new KeyValuePair<Type, Func<JsonElement, object>>(
                    typeof(JsonImageChannel),
                    (JsonElement el) =>
                    {
                        var elements = new JsonImageChannel();
                        if (el.TryGetProperty("R", out JsonElement R))
                            elements.R = R;
                        if (el.TryGetProperty("G", out JsonElement G))
                            elements.G = G;
                        if (el.TryGetProperty("B", out JsonElement B))
                            elements.B = B;
                        if (el.TryGetProperty("A", out JsonElement A))
                            elements.A = A;
                        return elements;
                    }
                )
            );

            #endregion

            #region MRT Renderer

            _meshRenderPassMRT = new API.RenderPass(
                API.Handler.MainDevice,
                new API.RenderPass.CreateInfo[]
                {
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo(),
                    new API.RenderPass.CreateInfo()
                },
                new API.RenderPass.CreateInfo()
            );

            _meshDescriptorSetLayouts = new API.DescriptorSetLayout[]
            {
                //PROJECTION
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //VIEW
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //MODEL
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                )
            };

            #endregion

            #region light

            _lightRenderPass = new API.RenderPass(
                API.Handler.MainDevice,
                new API.RenderPass.CreateInfo[]
                {
                    new API.RenderPass.CreateInfo()
                }
            );

            _lightDescriptorSetLayouts = new API.DescriptorSetLayout[]
            {
                //PROJECTION
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //VIEW
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                //MODEL
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Vertex,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                )
            };

            #endregion

            #region Deffered

            _defferedRenderPass = new API.RenderPass(
                API.Handler.MainDevice,
                new API.RenderPass.CreateInfo[]
                {
                    new API.RenderPass.CreateInfo()
                }
            );
            _defferedDescriptorSetLayouts = new API.DescriptorSetLayout[]
            {
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        //color
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        },
                        //normal
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        },
                        //position
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        },
                        //detail
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.CombinedImageSampler
                        }
                    }
                ),
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                ),
                new API.DescriptorSetLayout(
                    API.Handler.MainDevice,
                    new API.DescriptorSetCreateInfo[]
                    {
                        new API.DescriptorSetCreateInfo()
                        {
                            stage = API.ShaderStageType.Fragment,
                            type = API.DescriptorType.UniformBuffer
                        }
                    }
                )
            };

            #endregion
        }

        public override void Update()
        {
        }
    }
}