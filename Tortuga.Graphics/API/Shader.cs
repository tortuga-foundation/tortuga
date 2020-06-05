using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Numerics;

namespace Tortuga.Graphics.API
{
    /// <summary>
    /// Shader module used in pipeline
    /// </summary>
    public class Shader
    {
        /// <summary>
        /// Shader type
        /// </summary>
        public enum ShaderType
        {
            /// <summary>
            /// Vertex
            /// </summary>
            Vertex,
            /// <summary>
            /// Fragment
            /// </summary>
            Fragment,
            /// <summary>
            /// Compute
            /// </summary>
            Compute,
            /// <summary>
            /// TessellationEvaluation
            /// </summary>
            TessellationEvaluation,
            /// <summary>
            /// TessellationControl
            /// </summary>
            TessellationControl,
            /// <summary>
            /// Geometry
            /// </summary>
            Geometry
        };

        /// <summary>
        /// Shader specialization info
        /// </summary>
        public class Specialization
        {
            /// <summary>
            /// Unique identifier that matches the shader code specilization id
            /// </summary>
            public uint Identifier;
            /// <summary>
            /// size of the data
            /// </summary>
            public uint Size;
            /// <summary>
            /// data in bytes array
            /// </summary>
            public byte[] Data;
        }

        /// <summary>
        /// vulkan shader handle
        /// </summary>
        public VkShaderModule Handle => _shader;
        /// <summary>
        /// the device this shader is loaded on
        /// </summary>
        public Device DeviceUsed => _device;
        /// <summary>
        /// type of shader
        /// </summary>
        public ShaderType Type => _type;
        /// <summary>
        /// specializations applied to this shader
        /// </summary>
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

        /// <summary>
        /// Load shader from file
        /// </summary>
        /// <param name="device">vulkan device</param>
        /// <param name="file">shader file path</param>
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

        /// <summary>
        /// Load shader from string
        /// </summary>
        /// <param name="device">vulkan device</param>
        /// <param name="code">shader code</param>
        /// <param name="type">shader type</param>
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

        /// <summary>
        /// Load shader from spv
        /// </summary>
        /// <param name="device">vulkan device</param>
        /// <param name="compiledCode">compiled shader code</param>
        /// <param name="type">shader type</param>
        public Shader(Device device, byte[] compiledCode, ShaderType type)
        {
            _specialization = new List<Specialization>();
            _type = type;
            _device = device;
            SetupShader(compiledCode);
        }

        /// <summary>
        /// deconstructor for shader
        /// </summary>
        unsafe ~Shader()
        {
            Dispose();
        }

        /// <summary>
        /// destroy shader module
        /// </summary>
        public unsafe void Dispose()
        {
            if (_shader != VkShaderModule.Null)
            {
                vkDestroyShaderModule(
                    _device.LogicalDevice,
                    _shader,
                    null
                );
                _shader = VkShaderModule.Null;
            }
        }

        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="bytes">specialization data</param>
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
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
        public void CreateOrUpdateSpecialization(uint identifier, int data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
        public void CreateOrUpdateSpecialization(uint identifier, float data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
        public void CreateOrUpdateSpecialization(uint identifier, uint data)
        {
            var bytes = BitConverter.GetBytes(data);
            CreateOrUpdateSpecialization(identifier, bytes);   
        }
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
        public void CreateOrUpdateSpecialization(uint identifier, Vector2 data)
        {
            var bytes = new List<byte>();
            foreach (var b in BitConverter.GetBytes(data.X))
                bytes.Add(b);
            foreach (var b in BitConverter.GetBytes(data.Y))
                bytes.Add(b);
            CreateOrUpdateSpecialization(identifier, bytes.ToArray());   
        }
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
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
        /// <summary>
        /// apply shader specilization
        /// </summary>
        /// <param name="identifier">unique identifier</param>
        /// <param name="data">specialization data</param>
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

        /// <summary>
        /// Remove a specilization from from shader
        /// </summary>
        /// <param name="identifier">specialization identifier</param>
        public void DeleteSpecialization(uint identifier)
        {
            var index = _specialization.FindIndex(s => s.Identifier == identifier);
            if (index > -1)
                _specialization.RemoveAt(index);
        }

        /// <summary>
        /// get shader extension from shader type
        /// </summary>
        /// <param name="type">shader type</param>
        /// <returns>shader extension</returns>
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

        /// <summary>
        /// get shader type from file path
        /// </summary>
        /// <param name="file">file path</param>
        /// <returns>shader type</returns>
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