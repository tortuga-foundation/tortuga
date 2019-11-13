#include "Mesh.hpp"

namespace Tortuga
{
namespace Components
{

void Mesh::OnCreate()
{
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    //create buffers
    this->VertexStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(Graphics::Vertex) * Vertices.size(), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->VertexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, VertexStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    this->IndexStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(uint8_t) * Indices.size(), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
    this->IndexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, IndexStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT);
    //transfer
    this->TransferCommandPool = Graphics::Vulkan::CommandPool::Create(device, device.QueueFamilies.Transfer.Index);
    this->TransferVertexCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    this->TransferIndexCommand = Graphics::Vulkan::Command::Create(device, this->TransferCommandPool, Graphics::Vulkan::Command::PRIMARY);
    //record vertex
    Graphics::Vulkan::Command::Begin(this->TransferVertexCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferVertexCommand, this->VertexStagingBuffer, this->VertexBuffer);
    Graphics::Vulkan::Command::End(this->TransferVertexCommand);
    //record index
    Graphics::Vulkan::Command::Begin(this->TransferIndexCommand, VK_COMMAND_BUFFER_USAGE_SIMULTANEOUS_USE_BIT);
    Graphics::Vulkan::Command::CopyBuffer(this->TransferIndexCommand, this->IndexStagingBuffer, this->IndexBuffer);
    Graphics::Vulkan::Command::End(this->TransferIndexCommand);
    //update buffers
    Graphics::Vulkan::Buffer::SetData(this->VertexStagingBuffer, this->Vertices.data(), this->VertexStagingBuffer.Size);
    Graphics::Vulkan::Buffer::SetData(this->IndexStagingBuffer, this->Indices.data(), this->IndexStagingBuffer.Size);
    Graphics::Vulkan::Command::Submit({this->TransferVertexCommand, this->TransferIndexCommand}, device.Queues.Transfer[0]);
    this->ShouldReCreateBuffers = -1;
}
void Mesh::OnDestroy()
{
    Graphics::Vulkan::Buffer::Destroy(VertexStagingBuffer);
    Graphics::Vulkan::Buffer::Destroy(VertexBuffer);
    Graphics::Vulkan::Buffer::Destroy(IndexStagingBuffer);
    Graphics::Vulkan::Buffer::Destroy(IndexBuffer);
}
void Mesh::_ReCreateBuffers()
{
    if (this->ShouldReCreateBuffers == -1)
        return;
    const auto device = Core::Engine::GetPrimaryVulkanDevice();
    const auto type = this->ShouldReCreateBuffers;
    if (type == BUFFER_CREATION_ALL || type == BUFFER_CREATION_VERTEX)
    {
        Graphics::Vulkan::Buffer::Destroy(VertexStagingBuffer);
        Graphics::Vulkan::Buffer::Destroy(VertexBuffer);
        this->VertexStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(Graphics::Vertex) * Vertices.size(), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
        this->VertexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, VertexStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
    }

    if (type == BUFFER_CREATION_ALL || type == BUFFER_CREATION_INDEX)
    {
        Graphics::Vulkan::Buffer::Destroy(IndexStagingBuffer);
        Graphics::Vulkan::Buffer::Destroy(IndexBuffer);
        this->IndexStagingBuffer = Graphics::Vulkan::Buffer::CreateHost(device, sizeof(uint8_t) * Indices.size(), VK_BUFFER_USAGE_TRANSFER_SRC_BIT);
        this->IndexBuffer = Graphics::Vulkan::Buffer::CreateDevice(device, IndexStagingBuffer.Size, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT);
    }

    if (type == BUFFER_CREATION_ALL)
        Graphics::Vulkan::Command::Submit({this->TransferVertexCommand, this->TransferIndexCommand}, device.Queues.Transfer[0]);
    else if (type == BUFFER_CREATION_VERTEX)
        Graphics::Vulkan::Command::Submit({this->TransferVertexCommand}, device.Queues.Transfer[0]);
    else if (type == BUFFER_CREATION_INDEX)
        Graphics::Vulkan::Command::Submit({this->TransferIndexCommand}, device.Queues.Transfer[0]);

    this->ShouldReCreateBuffers = -1;
}
void Mesh::UpdateBuffers(BufferCreationType type)
{
    this->ShouldReCreateBuffers = type;
}

std::vector<Graphics::Vertex>
Mesh::GetVertices()
{
    return this->Vertices;
}
std::vector<uint8_t> Mesh::GetIndices()
{
    return this->Indices;
}
void Mesh::SetVertices(std::vector<Graphics::Vertex> vertices)
{
    this->Vertices = vertices;
}
void Mesh::SetIndices(std::vector<uint8_t> indices)
{
    this->Indices = indices;
}

Mesh::Mesh() {}
Mesh::Mesh(Utils::IO::OBJ obj)
{
    this->Vertices.resize(obj.Positions.size());
    this->Indices.resize(obj.Indices.size());
    for (uint32_t i = 0; i < obj.Indices.size(); i++)
    {
        this->Indices[i] = obj.Indices[i].Position;
        this->Vertices[obj.Indices[i].Position].Position = obj.Positions[obj.Indices[i].Position];
        this->Vertices[obj.Indices[i].Position].Texture = obj.Textures[obj.Indices[i].Texture];
        this->Vertices[obj.Indices[i].Position].Normal = obj.Normals[obj.Indices[i].Normal];
    }
    //compute tangent & bi tangents
    for (uint32_t i = 0; i < this->Indices.size(); i += 3)
    {
        const auto v0 = this->Vertices[this->Indices[i + 0]];
        const auto v1 = this->Vertices[this->Indices[i + 1]];
        const auto v2 = this->Vertices[this->Indices[i + 2]];

        const auto edge1 = v1.Position - v0.Position;
        const auto edge2 = v2.Position - v0.Position;

        const float deltaU1 = v1.Texture.x - v0.Texture.x;
        const float deltaV1 = v1.Texture.y - v0.Texture.y;
        const float deltaU2 = v2.Texture.x - v0.Texture.x;
        const float deltaV2 = v2.Texture.y - v0.Texture.y;

        const float f = 1.0f / (deltaU1 * deltaV2 - deltaU2 * deltaV1);
        glm::vec3 tangent, bitangent;
        tangent.x = f * (deltaV2 * edge1.x - deltaV1 * edge2.x);
        tangent.y = f * (deltaV2 * edge1.y - deltaV1 * edge2.y);
        tangent.z = f * (deltaV2 * edge1.z - deltaV1 * edge2.z);

        bitangent.x = f * (-deltaU2 * edge1.x - deltaU1 * edge2.x);
        bitangent.y = f * (-deltaU2 * edge1.y - deltaU1 * edge2.y);
        bitangent.z = f * (-deltaU2 * edge1.z - deltaU1 * edge2.z);

        this->Vertices[this->Indices[i + 0]].Tangent += tangent;
        this->Vertices[this->Indices[i + 1]].Tangent += tangent;
        this->Vertices[this->Indices[i + 2]].Tangent += tangent;

        this->Vertices[this->Indices[i + 0]].BiTangent += bitangent;
        this->Vertices[this->Indices[i + 1]].BiTangent += bitangent;
        this->Vertices[this->Indices[i + 2]].BiTangent += bitangent;
    }
    //normalize tangents
    for (uint32_t i = 0; i < this->Vertices.size(); i++)
    {
        this->Vertices[i].Tangent = glm::normalize(this->Vertices[i].Tangent);
        this->Vertices[i].BiTangent = glm::normalize(this->Vertices[i].BiTangent);
    }
}
} // namespace Components
} // namespace Tortuga