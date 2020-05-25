using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Numerics;

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

        public class Specialization
        {
            public uint Identifier;
            public uint Size;
            public byte[] Data;
        }

        public VkShaderModule Handle => _shader;
        public Device DeviceUsed => _device;
        public ShaderType Type => _type;
        public Specialization[] Specializations => _specialization.ToArray();

        private VkShaderModule _shader;
        private Device _device;
        private ShaderType _type;
        private List<Specialization> _specialization;

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
            _specialization = new List<Specialization>();
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
            _specialization = new List<Specialization>();
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
            _specialization = new List<Specialization>();
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

        private void CreateOrUpdateSpecialization(uint identifier, byte[] bytes)
        {
            var size = Convert.ToUInt32(bytes.Length * sizeof(byte));
            //does specialization exist
            var index = _specialization.FindIndex(s => s.Identifier == identifier);
            if (index == -1)
            {
                //create new specialization
                _specialization.Add(new Specialization()
                {
                    Identifier = identifier,
                    Size = size,
                    Data = bytes
                });
            }
            else
            {
                //update specialization
                _specialization[index] = new Specialization()
                {
                    Identifier = identifier,
                    Size = size,
                    Data = bytes
                };
            }
        }
        public void CreateOrUpdateSpecialization(uint identifier, int data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        public void CreateOrUpdateSpecialization(uint identifier, float data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        public void CreateOrUpdateSpecialization(uint identifier, uint data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        public void CreateOrUpdateSpecialization(uint identifier, Vector2 data)
        {
            var bytes = new List<byte>();
            foreach (var b in BitConverter.GetBytes(data.X))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Y))
                bytes.Add(b);
            CreateOrUpdateSpecialization(identifier, bytes.ToArray());   
        }
        public void CreateOrUpdateSpecialization(uint identifier, Vector3 data)
        {
            var bytes = new List<byte>();
            foreach (var b in BitConverter.GetBytes(data.X))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Y))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Z))
                bytes.Add(b);
            CreateOrUpdateSpecialization(identifier, bytes.ToArray());   
        }
        public void CreateOrUpdateSpecialization(uint identifier, Vector4 data)
        {
            var bytes = new List<byte>();
            foreach (var b in BitConverter.GetBytes(data.X))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Y))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Z))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.W))
                bytes.Add(b);
            CreateOrUpdateSpecialization(identifier, bytes.ToArray());   
        }

        public void DeleteSpecialization(uint identifier)
        {
            var index = _specialization.FindIndex(s => s.Identifier == identifier);
            if (index > -1)
                _specialization.RemoveAt(index);
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

        /// <summary>
        /// compiles shader code in a file and returns a path to compiled shader file
        /// </summary>
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