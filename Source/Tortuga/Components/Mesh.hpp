#ifndef _MESH_COMPONENT
#define _MESH_COMPONENT

#include <vector>
#include <cstring>
#include <glm/glm.hpp>

#include "../Core/Engine.hpp"
#include "../Graphics/Vertex.hpp"
#include "../Utils/IO.hpp"

namespace Tortuga
{
namespace Components
{
struct Mesh : public Core::ECS::Component
{
private:
  std::vector<Graphics::Vertex> Vertices;
  std::vector<uint32_t> Indices;
  bool IsVerticesDirty = false;
  bool IsIndicesDirty = false;

public:
  std::vector<Graphics::Vertex> GetVertices()
  {
    return this->Vertices;
  }
  std::vector<uint32_t> GetIndices()
  {
    return this->Indices;
  }
  void SetVertices(std::vector<Graphics::Vertex> vertices)
  {
    this->Vertices = vertices;
    this->IsVerticesDirty = true;
  }
  void SetIndices(std::vector<uint32_t> indices)
  {
    this->Indices = indices;
    this->IsIndicesDirty = true;
  }
  bool GetIsVerticesDirty()
  {
    return this->IsVerticesDirty;
  }
  bool GetIsIndicesDirty()
  {
    return this->IsIndicesDirty;
  }
  void SetDirty(bool vertices, bool indices)
  {
    this->IsVerticesDirty = vertices;
    this->IsIndicesDirty = indices;
  }

  Mesh()
  {
  }
  Mesh(Utils::IO::OBJ obj)
  {
    this->IsVerticesDirty = true;
    this->IsIndicesDirty = true;
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
};
} // namespace Components
} // namespace Tortuga

#endif