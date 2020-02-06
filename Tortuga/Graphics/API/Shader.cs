using System;
using System.IO;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Shader
    {
        public VkShaderModule Handle => _shader;
        private VkShaderModule _shader;

        private unsafe void SetupShader(byte[] byteCode)
        {
            fixed (byte* byteCodePtr = byteCode)
            {
                var shaderInfo = VkShaderModuleCreateInfo.New();
                shaderInfo.codeSize = new UIntPtr((uint)byteCode.Length);
                shaderInfo.pCode = (uint*)byteCodePtr;

                VkShaderModule shaderModule;
                if (vkCreateShaderModule(
                    Engine.Instance.MainDevice.LogicalDevice,
                    &shaderInfo,
                    null,
                    &shaderModule
                ) != VkResult.Success)
                    throw new Exception("failed to create vulkan shader module");
                _shader = shaderModule;
            }
        }

        public Shader(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException("could not find the shader file");
            var byteCode = File.ReadAllBytes(file);
            SetupShader(byteCode);
        }
        public unsafe Shader(byte[] byteCode)
            => SetupShader(byteCode);
        unsafe ~Shader()
        {
            vkDestroyShaderModule(
                Engine.Instance.MainDevice.LogicalDevice,
                _shader,
                null
            );
        }

        public static byte[] Compile(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException("failed to find shader file");

            return File.ReadAllBytes(file);
        }
    }
}