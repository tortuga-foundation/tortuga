using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    internal class Shader
    {
        public enum ShaderType
        {
            Vertex,
            Fragment,
            Compute,
            TessellationEvaluation,
            TessellationControl,
            Geometry
        };

        public VkShaderModule Handle => _shader;
        public Device DeviceUsed => _device;
        public ShaderType Type => _type;

        private VkShaderModule _shader;
        private Device _device;
        private ShaderType _type;

        private unsafe void SetupShader(byte[] byteCode)
        {
            fixed (byte* byteCodePtr = byteCode)
            {
                var shaderInfo = VkShaderModuleCreateInfo.New();
                shaderInfo.codeSize = new UIntPtr((uint)byteCode.Length);
                shaderInfo.pCode = (uint*)byteCodePtr;

                VkShaderModule shaderModule;
                if (vkCreateShaderModule(
                    _device.LogicalDevice,
                    &shaderInfo,
                    null,
                    &shaderModule
                ) != VkResult.Success)
                    throw new Exception("failed to create vulkan shader module");
                _shader = shaderModule;
            }
        }

        public Shader(Device device, string file)
        {
            _type = GetShaderTypeFromExtension(file);
            _device = device;
            var compileTask = Compile(file);
            compileTask.Wait();
            var compiledCode = File.ReadAllBytes(compileTask.Result);
            SetupShader(compiledCode);
            File.Delete(compileTask.Result);
        }
        public Shader(Device device, string code, ShaderType type)
        {
            _type = type;
            _device = device;
            var shaderFile = string.Format(
                "{0}{1}.{2}",
                Path.GetTempPath(),
                Guid.NewGuid().ToString(),
                GetShaderTypeExtension(type)
            );
            File.Create(shaderFile);
            File.AppendAllText(shaderFile, code);
            var compileTask = Compile(shaderFile);
            compileTask.Wait();
            var compiledCode = File.ReadAllBytes(compileTask.Result);
            SetupShader(compiledCode);

            //clean up
            File.Delete(shaderFile);
            File.Delete(compileTask.Result);
        }
        public Shader(Device device, byte[] compiledCode, ShaderType type)
        {
            _type = type;
            _device = device;
            SetupShader(compiledCode);
        }
        unsafe ~Shader()
        {
            vkDestroyShaderModule(
                _device.LogicalDevice,
                _shader,
                null
            );
        }

        public string GetShaderTypeExtension(ShaderType type)
        {
            switch (type)
            {
                case ShaderType.Vertex:
                    return "vert";
                case ShaderType.Fragment:
                    return "frag";
                case ShaderType.Compute:
                    return "comp";
                case ShaderType.Geometry:
                    return "geom";
                case ShaderType.TessellationControl:
                    return "tesc";
                case ShaderType.TessellationEvaluation:
                    return "tese";
            }
            throw new NotSupportedException("this type of shader is not supported");
        }

        public ShaderType GetShaderTypeFromExtension(string file)
        {
            if (file.EndsWith("vert"))
                return ShaderType.Vertex;
            else if (file.EndsWith("frag"))
                return ShaderType.Fragment;
            else if (file.EndsWith("comp"))
                return ShaderType.Compute;
            else if (file.EndsWith("geom"))
                return ShaderType.Geometry;
            else if (file.EndsWith("tesc"))
                return ShaderType.TessellationControl;
            else if (file.EndsWith("tese"))
                return ShaderType.TessellationEvaluation;
            else
                throw new Exception("invalid shader extension type");
        }

        ///compiles shader code in a file and returns a path to compiled shader file
        public static async Task<string> Compile(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException("failed to find shader file");

            var outFile = string.Format(
                "{0}{1}.{2}",
                Path.GetTempPath(),
                Guid.NewGuid().ToString(),
                "spv"
            );

            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "glslangValidator";
            startInfo.Arguments = string.Format(
                "{0} -V -o {1}",
                file,
                outFile
            );
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return await Task.FromResult(outFile);
        }
    }
}