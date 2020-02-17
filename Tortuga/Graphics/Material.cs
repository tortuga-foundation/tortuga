using Vulkan;
using Tortuga.Graphics.API;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Tortuga.Graphics
{
    public class Material
    {
        public Matrix4x4 Model;

        internal Shader Vertex => _vertex;
        internal Shader Fragment => _fragment;
        internal Pipeline ActivePipeline => _pipeline;
        internal List<DescriptorSetPool.DescriptorSet> DescriptorSets => _descriptorSets;

        private Shader _vertex;
        private Shader _fragment;
        private Pipeline _pipeline;
        private List<DescriptorSetLayout> _layouts;
        private List<DescriptorSetPool> _setPool;
        private List<DescriptorSetPool.DescriptorSet> _descriptorSets;
        private List<Buffer> _setBuffers;
        private bool _isDirty;

        public Material(string vertexShader, string fragmentShader)
        {
            _vertex = new Shader(vertexShader);
            _fragment = new Shader(fragmentShader);

            _layouts = new List<DescriptorSetLayout>();
            _setPool = new List<DescriptorSetPool>();
            _descriptorSets = new List<DescriptorSetPool.DescriptorSet>();
            _setBuffers = new List<Buffer>();

            //descriptor sets
            AddDescriptorSet<Matrix4x4>(new DescriptorSetCreateInfo[]{
                new DescriptorSetCreateInfo
                {
                    stage = VkShaderStageFlags.All,
                    type = VkDescriptorType.UniformBuffer
                }
            });
        }

        public void ReCompilePipeline()
        {
            if (_isDirty == false)
                return;

            var totalDescriptorSets = new List<DescriptorSetLayout>();
            totalDescriptorSets.Add(Engine.Instance.CameraDescriptorLayout);
            foreach (var l in _layouts)
                totalDescriptorSets.Add(l);

            _pipeline = new Pipeline(
                totalDescriptorSets.ToArray(),
                _vertex,
                _fragment
            );
        }

        public async Task UpdateModel(Matrix4x4 model)
        {
            await _setBuffers[0].SetDataWithStaging(new Matrix4x4[] { model });
        }

        public void UpdateShaders(string vertex, string fragment)
        {
            _vertex = new Shader(vertex);
            _fragment = new Shader(fragment);
            _isDirty = true;
        }

        public void AddDescriptorSet<T>(DescriptorSetCreateInfo[] createInfo)
        {
            var layout = new DescriptorSetLayout(createInfo);
            var pool = new DescriptorSetPool(layout);
            var set = pool.AllocateDescriptorSet();
            var buffer = Buffer.CreateDevice(
                System.Convert.ToUInt32(Unsafe.SizeOf<T>()),
                VkBufferUsageFlags.UniformBuffer | VkBufferUsageFlags.TransferDst
            );
            set.BuffersUpdate(new Buffer[] { buffer });
            _layouts.Add(layout);
            _setPool.Add(pool);
            _descriptorSets.Add(set);
            _setBuffers.Add(buffer);
            _isDirty = true;
        }
    }
}