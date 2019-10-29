#include "./Rendering.hpp"

namespace Tortuga
{
namespace Systems
{
void Rendering::MeshView::Setup(Graphics::Vulkan::Device::Device device, std::vector<Graphics::Vulkan::DescriptorLayout::DescriptorLayout> layouts)
{
  this->DeviceInUse = device;
  if (this->Root == nullptr)
    return;
  const auto mesh = Core::Engine::GetComponent<Components::Mesh>(this->Root);
  const auto transform = Core::Engine::GetComponent<Components::Transform>(this->Root);
  const auto material = Core::Engine::GetComponent<Components::Material>(this->Root);
  if (mesh == nullptr)
    return;
  this->IsStatic = true;
  if (transform != nullptr)
    this->IsStatic = transform->GetStatic();
  const uint32_t verticesSize = sizeof(Graphics::Vertex) * mesh->GetVertices().size();
  const uint32_t indicesSize = sizeof(uint32_t) * mesh->GetIndices().size();
  this->IsTransformDirty = false;

  //setup staging transform buffer
  if (this->StagingTransformBuffer.Buffer == VK_NULL_HANDLE)
  {
    this->StagingTransformBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(glm::mat4), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->TransformBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, sizeof(glm::mat4), VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    IsTransformDirty = true;
  }
  //setup descriptor pools
  if (this->DescriptorPool.Pool == VK_NULL_HANDLE)
    this->DescriptorPool = Graphics::Vulkan::DescriptorPool::Create(device, layouts, 3);

  if (this->TransformDescriptorSet.set == VK_NULL_HANDLE)
  {
    this->TransformDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, this->DescriptorPool, layouts[1]);
    Graphics::Vulkan::DescriptorSet::UpdateDescriptorSet(this->TransformDescriptorSet, {this->TransformBuffer});
  }

  //update mesh transform data
  if (!this->IsStatic || this->IsTransformDirty)
  {
    glm::mat4 matrix = glm::mat4(1.0f);
    if (transform != nullptr)
      matrix = transform->GetModelMatrix();

    Graphics::Vulkan::Buffer::SetData(StagingTransformBuffer, &matrix, StagingTransformBuffer.Size);
  }

  //update mesh vertices
  this->IsMeshDirty = false;
  if (this->StagingVertexBuffer.Buffer == VK_NULL_HANDLE || mesh->GetIsVerticesDirty())
  {
    if (this->StagingVertexBuffer.Buffer == VK_NULL_HANDLE || this->StagingVertexBuffer.Size != verticesSize)
    {
      Graphics::Vulkan::Buffer::Destroy(this->StagingVertexBuffer);
      Graphics::Vulkan::Buffer::Destroy(this->VertexBuffer);
      this->StagingVertexBuffer = Graphics::Vulkan::Buffer::CreateHost(device, verticesSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
      this->VertexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, verticesSize, VK_BUFFER_USAGE_VERTEX_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    }
    Graphics::Vulkan::Buffer::SetData(this->StagingVertexBuffer, mesh->GetVertices().data(), verticesSize);
    this->IsMeshDirty = true;
  }

  //update mesh indices
  if (this->StagingIndexBuffer.Buffer == VK_NULL_HANDLE || mesh->GetIsIndicesDirty())
  {
    if (this->StagingIndexBuffer.Buffer == VK_NULL_HANDLE || this->StagingIndexBuffer.Size != indicesSize)
    {
      Graphics::Vulkan::Buffer::Destroy(this->StagingIndexBuffer);
      Graphics::Vulkan::Buffer::Destroy(this->IndexBuffer);
      this->StagingIndexBuffer = Graphics::Vulkan::Buffer::CreateHost(device, indicesSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
      this->IndexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, indicesSize, VK_BUFFER_USAGE_INDEX_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    }
    Graphics::Vulkan::Buffer::SetData(this->StagingIndexBuffer, mesh->GetIndices().data(), indicesSize);
    this->IndexCount = mesh->GetIndices().size();
    this->IsMeshDirty = true;
  }
  mesh->SetDirty(false, false);

  //setup commands & command pools
  if (this->GraphicsCommandPool.CommandPool == VK_NULL_HANDLE)
    this->GraphicsCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Graphics.Index);
  if (this->RenderCommand.Command == VK_NULL_HANDLE)
    this->RenderCommand = Graphics::Vulkan::Command::Create(device, this->GraphicsCommandPool, Graphics::Vulkan::Command::SECONDARY);
  if (this->TransferCommandPool.CommandPool == VK_NULL_HANDLE)
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
  if (this->MeshTransferCommand.Command == VK_NULL_HANDLE)
  {
    this->MeshTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->MeshTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->MeshTransferCommand, this->StagingVertexBuffer, this->VertexBuffer);
    Graphics::Vulkan::Command::CopyBuffer(this->MeshTransferCommand, this->StagingIndexBuffer, this->IndexBuffer);
    Graphics::Vulkan::Command::End(this->MeshTransferCommand);
  }
  if (this->TransformTransferCommand.Command == VK_NULL_HANDLE)
  {
    this->TransformTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->TransformTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransformTransferCommand, this->StagingTransformBuffer, this->TransformBuffer);
    Graphics::Vulkan::Command::End(this->TransformTransferCommand);
  }

  //setup lights info
  if (this->StagingLightsBuffer.Buffer == VK_NULL_HANDLE)
  {
    uint32_t lightsByteSize = sizeof(glm::vec4) + (sizeof(Rendering::LightInfoStruct) * MAXIMUM_NUM_OF_LIGHTS);
    this->StagingLightsBuffer = Graphics::Vulkan::Buffer::CreateHost(device, lightsByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->LightsBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, lightsByteSize, VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    this->LightsDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, this->DescriptorPool, layouts[2]);
    Graphics::Vulkan::DescriptorSet::UpdateDescriptorSet(this->LightsDescriptorSet, {this->LightsBuffer});
    this->LightTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->LightTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->LightTransferCommand, this->StagingLightsBuffer, this->LightsBuffer);
    Graphics::Vulkan::Command::End(this->LightTransferCommand);
  }

  //setup material info
  if (this->StagingMaterialBuffer.Buffer == VK_NULL_HANDLE)
  {
    uint32_t materialByteSize = sizeof(glm::vec4) + sizeof(float) + sizeof(float);
    this->StagingMaterialBuffer = Graphics::Vulkan::Buffer::CreateHost(device, materialByteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->MaterialBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, materialByteSize, VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
    this->MaterialDescriptorSet = Graphics::Vulkan::DescriptorSet::Create(device, this->DescriptorPool, layouts[3]);
    Graphics::Vulkan::DescriptorSet::UpdateDescriptorSet(this->MaterialDescriptorSet, {this->MaterialBuffer});
    this->MaterialTransferCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    Graphics::Vulkan::Command::Begin(this->MaterialTransferCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->MaterialTransferCommand, this->StagingMaterialBuffer, this->MaterialBuffer);
    Graphics::Vulkan::Command::End(this->MaterialTransferCommand);
    this->IsMaterialDirty = true;
  }

  if (material != nullptr)
  {
    if (material->GetIsDirty())
      this->IsMaterialDirty = true;
    if (this->IsMaterialDirty)
    {
      glm::vec4 color = glm::vec4(material->GetColor(), 1.0f);
      float metallic = material->GetMetalic();
      float roughness = material->GetRoughness();
      Graphics::Vulkan::Buffer::SetData(this->StagingMaterialBuffer, &color, sizeof(glm::vec4));
      Graphics::Vulkan::Buffer::SetData(this->StagingMaterialBuffer, &metallic, sizeof(float), sizeof(glm::vec4));
      Graphics::Vulkan::Buffer::SetData(this->StagingMaterialBuffer, &roughness, sizeof(float), sizeof(glm::vec4) + sizeof(float));
    }
  }
}
void Rendering::MeshView::OnDestroy()
{
  Graphics::Vulkan::Buffer::Destroy(this->StagingMaterialBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->MaterialBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->StagingVertexBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->VertexBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->StagingIndexBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->IndexBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->StagingTransformBuffer);
  Graphics::Vulkan::Buffer::Destroy(this->TransformBuffer);
  Graphics::Vulkan::CommandPool::Destroy(this->GraphicsCommandPool);
  Graphics::Vulkan::CommandPool::Destroy(this->TransferCommandPool);
  Graphics::Vulkan::DescriptorPool::Destroy(this->DescriptorPool);
}
std::vector<Components::Light *> Rendering::MeshView::GetClosestLights()
{
  const auto fullLightsList = Core::Engine::GetComponents<Components::Light>();
  if (fullLightsList.size() <= Rendering::LightsPerMesh)
    return fullLightsList;

  const auto transform = Core::Engine::GetComponent<Components::Transform>(this->Root);
  auto position = glm::vec3(0, 0, 0);
  if (transform != nullptr)
    position = transform->GetPosition();

  /*
  std::sort(fullLightsList.begin(), fullLightsList.end(), [position](Components::Light *left, Components::Light *right) {
    const auto leftTransform = Core::Engine::GetComponent<Components::Transform>(left->Root);
    auto leftPosition = glm::vec3(0, 0, 0);
    if (leftTransform != nullptr)
      leftPosition = leftTransform->GetPosition();
    const auto rightTransform = Core::Engine::GetComponent<Components::Transform>(right->Root);
    auto rightPosition = glm::vec3(0, 0, 0);
    if (rightTransform != nullptr)
      rightPosition = rightTransform->GetPosition();
    return glm::distance(position, leftPosition) < glm::distance(position, rightPosition);
  });
  */

  std::vector<Components::Light *> lightsList(Rendering::LightsPerMesh);
  for (uint32_t i = 0; i < Rendering::LightsPerMesh; i++)
    lightsList[i] = fullLightsList[i];
  return lightsList;
}
void Rendering::MeshView::UpdateLightsBuffer(std::vector<Components::Light *> lights)
{
  if (lights.size() == 0)
    return;

  uint32_t lightsSize = lights.size();
  std::vector<Rendering::LightInfoStruct> lightInfos;
  for (const auto light : lights)
  {
    auto lightInfo = Rendering::LightInfoStruct();
    lightInfo.Color = light->GetColor();
    lightInfo.Intensity = light->GetIntensity();
    lightInfo.Range = light->GetRange();
    lightInfo.Type = light->GetType();

    const auto lightTransform = Core::Engine::GetComponent<Components::Transform>(light->Root);
    if (lightTransform != nullptr)
    {
      lightInfo.Forward = glm::vec4(lightTransform->GetForward(), 1.0f);
      lightInfo.Position = glm::vec4(lightTransform->GetPosition(), 1.0f);
    }
    lightInfos.push_back(lightInfo);
  }
  Graphics::Vulkan::Buffer::SetData(this->StagingLightsBuffer, &lightsSize, sizeof(uint32_t));
  Graphics::Vulkan::Buffer::SetData(this->StagingLightsBuffer, lightInfos.data(), sizeof(Rendering::LightInfoStruct) * lightsSize, sizeof(glm::vec4));
}
} // namespace Systems
} // namespace Tortuga