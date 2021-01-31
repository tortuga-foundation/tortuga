#pragma warning disable CS1591
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public class ShaderModule
    {
        public Device Device => _device;
        public VkShaderModule Handle => _handle;

        private Device _device;
        private VkShaderModule _handle;

        public ShaderModule(Device device, string filePath)
        {
            _device = device;
            var outFile = $"{Path.GetTempFileName()}.spv";
            CompileShader(filePath, outFile);
            var shaderCode = File.ReadAllBytes(outFile);
            Init(shaderCode);
        }
        public ShaderModule(Device device, byte[] compiledShaderCode)
        {
            _device = device;
            Init(compiledShaderCode);
        }

        unsafe ~ShaderModule()
        {
            if (_handle != VkShaderModule.Null)
            {
                VulkanNative.vkDestroyShaderModule(
                    _device.Handle,
                    _handle,
                    null
                );
                _handle = VkShaderModule.Null;
            }
        }

        public static void CompileShader(string inputFile, string outputFile)
        {
            if (File.Exists(inputFile) == false)
                throw new FileNotFoundException($"failed to find shader file {inputFile}");

            //setup process start info
            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"{inputFile} -V -o {outputFile}"
            };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                startInfo.FileName = "glslangValidator";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                startInfo.FileName = "glslangValidator.exe";
            else
                Console.Write("shader compilation is not supported on this platform");

            //start process
            var process = new Process
            {
                StartInfo = startInfo
            };
            process.Start();
            process.WaitForExit();
        }

        public unsafe void Init(byte[] compiledShaderCode)
        {
            fixed (byte* byteCodePtr = compiledShaderCode)
            {
                var shaderInfo = new VkShaderModuleCreateInfo
                {
                    sType = VkStructureType.ShaderModuleCreateInfo,
                    codeSize = new UIntPtr((uint)compiledShaderCode.Length),
                    pCode = (uint*)byteCodePtr
                };

                VkShaderModule shaderModule;
                if (VulkanNative.vkCreateShaderModule(
                    _device.Handle,
                    &shaderInfo,
                    null,
                    &shaderModule
                ) != VkResult.Success)
                    throw new Exception("failed to create shader module");
                _handle = shaderModule;
            }
        }
    }
}