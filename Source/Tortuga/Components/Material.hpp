#ifndef _MATERIAL_COMPONENT
#define _MATERIAL_COMPONENT

#include <glm/glm.hpp>

#include "../Core/ECS/Entity.hpp"

namespace Tortuga
{
namespace Components
{
struct Material : Core::ECS::Component
{
private:
  bool IsDirty = false;
  Graphics::Image Albedo;
  glm::vec3 Color = glm::vec3(0.4, 0.4, 0.4);
  float Metalic = 0.0f;
  float Roughness = 0.3f;

public:
  bool GetIsDirty()
  {
    return this->IsDirty;
  }
  void SetIsDirty(bool isDirty)
  {
    this->IsDirty = isDirty;
  }

  glm::vec3 GetColor()
  {
    return this->Color;
  }
  void SetColor(glm::vec3 color)
  {
    this->Color = color;
  }

  float GetMetalic()
  {
    return this->Metalic;
  }
  void SetMetalic(float metalic)
  {
    this->Metalic = metalic;
  }

  float GetRoughness()
  {
    return this->Roughness;
  }
  void SetRoughness(float roughness)
  {
    this->Roughness = roughness;
  }

  Graphics::Image GetAlbedo()
  {
    return this->Albedo;
  }
  void SetAlbedo(Graphics::Image image)
  {
    this->Albedo = image;
  }
};
} // namespace Components
} // namespace Tortuga

#endif