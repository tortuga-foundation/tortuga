using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Tortuga.Graphics.API
{
    public class Shader
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
        private VkShaderModule _shader;
        public ShaderType Type;

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
            var compileTask = Compile(file);
            compileTask.Wait();
            var compiledCode = File.ReadAllBytes(compileTask.Result);
            SetupShader(compiledCode);
            File.Delete(compileTask.Result);
        }
        public Shader(string code, ShaderType type)
        {
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
        public Shader(byte[] compiledCode) => SetupShader(compiledCode);
        unsafe ~Shader()
        {
            vkDestroyShaderModule(
                Engine.Instance.MainDevice.LogicalDevice,
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