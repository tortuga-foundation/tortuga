#pragma warning disable CS1591
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Vulkan;

namespace Tortuga.Graphics.API
{
    public enum ShaderType
    {
        Vertex = 1,
        TessellationControl = 2,
        TessellationEvaluation = 4,
        Geometry = 8,
        Fragment = 16,
        Compute = 32,
    }

    public class ShaderModule
    {
        public Device Device => _device;
        public VkShaderModule Handle => _handle;
        public ShaderType Type => _type;

        private Device _device;
        private VkShaderModule _handle;
        private ShaderType _type;

        public ShaderModule(Device device, string filePath)
        {
            ShaderType type;
            if (filePath.EndsWith(".vert"))
                type = ShaderType.Vertex;
            else if (filePath.EndsWith(".frag"))
                type = ShaderType.Fragment;
            else if (filePath.EndsWith(".geom"))
                type = ShaderType.Geometry;
            else if (filePath.EndsWith(".tesc"))
                type = ShaderType.TessellationControl;
            else if (filePath.EndsWith(".tese"))
                type = ShaderType.TessellationEvaluation;
            else if (filePath.EndsWith(".comp"))
                type = ShaderType.Compute;
            else
                throw new Exception("invalid file format");

            _device = device;
            var outFile = $"{Path.GetTempFileName()}.spv";
            CompileShader(filePath, outFile);
            var shaderCode = File.ReadAllBytes(outFile);
            Init(type, shaderCode);
        }
        public ShaderModule(Device device, ShaderType type, byte[] compiledShaderCode)
        {
            _device = device;
            Init(type, compiledShaderCode);
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

        public unsafe void Init(ShaderType type, byte[] compiledShaderCode)
        {
            _type = type;
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