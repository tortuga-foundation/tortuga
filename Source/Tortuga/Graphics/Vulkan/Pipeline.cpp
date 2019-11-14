#include "./Pipeline.hpp"

#include "../Vertex.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Pipeline
{

std::vector<VkDescriptorSetLayout> GetDescriptorSetLayouts(
    std::vector<DescriptorLayout::DescriptorLayout> descriptorLayouts)
{
  std::vector<VkDescriptorSetLayout> layouts(descriptorLayouts.size());
  for (uint32_t i = 0; i < descriptorLayouts.size(); i++)
    layouts[i] = descriptorLayouts[i].Layouts;
  return layouts;
}

Pipeline Create(
    Device::Device device,
    RenderPass::RenderPass renderPass,
    std::vector<Shader::Shader> shaders,
    std::vector<DescriptorLayout::DescriptorLayout> descriptorLayouts)
{
  auto data = Pipeline();
  data.Device = device.Device;
  data.RenderPass = renderPass.RenderPass;

  const auto vertexBindings = Graphics::Vertex::GetBindingDescription();
  const auto vertexAttributes = Graphics::Vertex::GetAttributeDescriptions();
  auto vertexInputInfo = VkPipelineVertexInputStateCreateInfo();
  {
    vertexInputInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;
    vertexInputInfo.vertexBindingDescriptionCount = vertexBindings.size();
    vertexInputInfo.pVertexBindingDescriptions = vertexBindings.data();
    vertexInputInfo.vertexAttributeDescriptionCount = vertexAttributes.size();
    vertexInputInfo.pVertexAttributeDescriptions = vertexAttributes.data();
  }

  auto inputAssembly = VkPipelineInputAssemblyStateCreateInfo();
  {
    inputAssembly.sType = VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO;
    inputAssembly.topology = VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST;
    inputAssembly.primitiveRestartEnable = VK_FALSE;
  }

  //these are dynamic and will be set when rendering the mesh
  auto viewport = VkViewport(); //un-used
  auto scissors = VkRect2D();   //un-used

  auto viewportState = VkPipelineViewportStateCreateInfo();
  {
    viewportState.sType = VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO;
    viewportState.viewportCount = 1;
    viewportState.pViewports = &viewport;
    viewportState.scissorCount = 1;
    viewportState.pScissors = &scissors;
  }

  auto rasterizer = VkPipelineRasterizationStateCreateInfo();
  {
    rasterizer.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
    rasterizer.depthClampEnable = VK_FALSE;
    rasterizer.rasterizerDiscardEnable = VK_FALSE;
    rasterizer.polygonMode = VK_POLYGON_MODE_FILL;
    rasterizer.lineWidth = 1.0f;
    rasterizer.cullMode = VK_CULL_MODE_BACK_BIT;
    rasterizer.frontFace = VK_FRONT_FACE_COUNTER_CLOCKWISE;
    rasterizer.depthBiasEnable = VK_FALSE;
    rasterizer.depthBiasConstantFactor = 0.0f;
    rasterizer.depthBiasClamp = 0.0f;
    rasterizer.depthBiasSlopeFactor = 0.0f;
  }

  auto multisampling = VkPipelineMultisampleStateCreateInfo();
  {
    multisampling.sType = VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO;
    multisampling.sampleShadingEnable = VK_FALSE;
    multisampling.rasterizationSamples = VK_SAMPLE_COUNT_1_BIT;
    multisampling.minSampleShading = 1.0f;
    multisampling.pSampleMask = nullptr;
    multisampling.alphaToCoverageEnable = VK_FALSE;
    multisampling.alphaToOneEnable = VK_FALSE;
  }

  auto colorBlendAttachment = VkPipelineColorBlendAttachmentState();
  {
    colorBlendAttachment.colorWriteMask = VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
    colorBlendAttachment.blendEnable = VK_TRUE;
    colorBlendAttachment.srcColorBlendFactor = VK_BLEND_FACTOR_SRC_ALPHA;
    colorBlendAttachment.dstColorBlendFactor = VK_BLEND_FACTOR_ONE_MINUS_SRC_ALPHA;
    colorBlendAttachment.colorBlendOp = VK_BLEND_OP_ADD;
    colorBlendAttachment.srcAlphaBlendFactor = VK_BLEND_FACTOR_ONE;
    colorBlendAttachment.dstAlphaBlendFactor = VK_BLEND_FACTOR_ZERO;
    colorBlendAttachment.alphaBlendOp = VK_BLEND_OP_ADD;
  }

  auto depthStencil = VkPipelineDepthStencilStateCreateInfo();
  {
    depthStencil.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
    depthStencil.depthTestEnable = VK_TRUE;
    depthStencil.depthWriteEnable = VK_TRUE;
    depthStencil.depthCompareOp = VK_COMPARE_OP_LESS;
    depthStencil.depthBoundsTestEnable = VK_FALSE;
    depthStencil.minDepthBounds = 0.0f;
    depthStencil.maxDepthBounds = 1.0f;
    depthStencil.stencilTestEnable = VK_FALSE;
    depthStencil.front = {};
    depthStencil.back = {};
  }

  auto colorBlending = VkPipelineColorBlendStateCreateInfo();
  {
    colorBlending.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
    colorBlending.logicOpEnable = VK_FALSE;
    colorBlending.logicOp = VK_LOGIC_OP_COPY;
    colorBlending.attachmentCount = 1;
    colorBlending.pAttachments = &colorBlendAttachment;
    colorBlending.blendConstants[0] = 0.0f;
    colorBlending.blendConstants[1] = 0.0f;
    colorBlending.blendConstants[2] = 0.0f;
    colorBlending.blendConstants[3] = 0.0f;
  }

  std::vector<VkDynamicState> dynamicStates = {
      VK_DYNAMIC_STATE_VIEWPORT,
      VK_DYNAMIC_STATE_SCISSOR,
      VK_DYNAMIC_STATE_LINE_WIDTH};
  auto dynamicState = VkPipelineDynamicStateCreateInfo();
  {
    dynamicState.sType = VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO;
    dynamicState.dynamicStateCount = dynamicStates.size();
    dynamicState.pDynamicStates = dynamicStates.data();
  }

  const auto vulkanDescriptorLayouts = GetDescriptorSetLayouts(descriptorLayouts);
  auto pipelineLayoutInfo = VkPipelineLayoutCreateInfo();
  {
    pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
    pipelineLayoutInfo.setLayoutCount = vulkanDescriptorLayouts.size();
    pipelineLayoutInfo.pSetLayouts = vulkanDescriptorLayouts.data();
    pipelineLayoutInfo.pushConstantRangeCount = 0;    // Optional
    pipelineLayoutInfo.pPushConstantRanges = nullptr; // Optional
  }
  ErrorCheck::Callback(vkCreatePipelineLayout(data.Device, &pipelineLayoutInfo, nullptr, &data.Layout));

  std::vector<VkPipelineShaderStageCreateInfo> shaderInfo(shaders.size());
  for (uint32_t i = 0; i < shaderInfo.size(); i++)
  {
    shaderInfo[i].sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
    shaderInfo[i].module = shaders[i].Shader;
    shaderInfo[i].stage = shaders[i].Type;
    shaderInfo[i].pName = "main";
  }

  auto pipelineInfo = VkGraphicsPipelineCreateInfo();
  {
    pipelineInfo.sType = VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO;
    pipelineInfo.stageCount = shaderInfo.size();
    pipelineInfo.pStages = shaderInfo.data();
    pipelineInfo.pVertexInputState = &vertexInputInfo;
    pipelineInfo.pInputAssemblyState = &inputAssembly;
    pipelineInfo.pViewportState = &viewportState;
    pipelineInfo.pRasterizationState = &rasterizer;
    pipelineInfo.pMultisampleState = &multisampling;
    pipelineInfo.pDepthStencilState = &depthStencil;
    pipelineInfo.pColorBlendState = &colorBlending;
    pipelineInfo.pDynamicState = &dynamicState;
    pipelineInfo.layout = data.Layout;
    pipelineInfo.renderPass = data.RenderPass;
    pipelineInfo.subpass = 0;
    pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
    pipelineInfo.basePipelineIndex = -1;
  }
  ErrorCheck::Callback(vkCreateGraphicsPipelines(data.Device, VK_NULL_HANDLE, 1, &pipelineInfo, VK_NULL_HANDLE, &data.Pipeline));
  return data;
}
void Destroy(Pipeline data)
{
  if (data.Pipeline != VK_NULL_HANDLE)
    vkDestroyPipeline(data.Device, data.Pipeline, VK_NULL_HANDLE);
  if (data.Layout != VK_NULL_HANDLE)
    vkDestroyPipelineLayout(data.Device, data.Layout, VK_NULL_HANDLE);
  data.Pipeline = VK_NULL_HANDLE;
  data.Layout = VK_NULL_HANDLE;
}
} // namespace Pipeline
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga