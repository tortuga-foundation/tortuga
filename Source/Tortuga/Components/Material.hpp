#ifndef _MATERIAL_COMPONENT
#define _MATERIAL_COMPONENT

#include <glm/glm.hpp>

#include "../Core/ECS/Entity.hpp"
#include "../Graphics/Image.hpp"

namespace Tortuga
{
namespace Components
{
struct Material : Core::ECS::Component
{
private:
  bool IsDirty = false;
  Graphics::Image Albedo = Graphics::Image::White();
  Graphics::Image Normal = Graphics::Image::Blue();
  Graphics::Image Detail1 = Graphics::Image::White();
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
    this->IsDirty = true;
  }

  float GetMetalic()
  {
    return this->Metalic;
  }
  void SetMetalic(float metalic)
  {
    this->Metalic = metalic;
    this->IsDirty = true;
  }

  float GetRoughness()
  {
    return this->Roughness;
  }
  void SetRoughness(float roughness)
  {
    this->Roughness = roughness;
    this->IsDirty = true;
  }

  Graphics::Image GetAlbedo()
  {
    return this->Albedo;
  }
  void SetAlbedo(Graphics::Image image)
  {
    this->Albedo = image;
    this->IsDirty = true;
  }
  Graphics::Image GetNormal()
  {
    return this->Normal;
  }
  void SetNormal(Graphics::Image image)
  {
    this->Normal = image;
    this->IsDirty = true;
  }
  Graphics::Image GetDetail1()
  {
    return this->Detail1;
  }
  void SetDetail1(Graphics::Image image)
  {
    this->Detail1 = image;
    this->IsDirty = true;
  }
};
} // namespace Components
} // namespace Tortuga

#endif