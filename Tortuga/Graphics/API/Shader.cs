using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Shader
    {
        private VkShaderModule _shader;

        public unsafe Shader(byte[] byteCode)
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

        unsafe ~Shader()
        {
            vkDestroyShaderModule(
                Engine.Instance.MainDevice.LogicalDevice,
                _shader,
                null
            );
        }
    }
}