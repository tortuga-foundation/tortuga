#ifndef _VULKAN_SHADER
#define _VULKAN_SHADER

#include <vulkan/vulkan.h>
#include <string>

#include "./ErrorCheck.hpp"
#include "./Instance.hpp"
#include "./Device.hpp"
#include "../../Utils/IO.hpp"

namespace Tortuga
{
namespace Graphics
{
namespace Vulkan
{
namespace Shader
{
struct Shader
{
  VkDevice Device = VK_NULL_HANDLE;
  VkShaderModule Shader = VK_NULL_HANDLE;
};

struct FullShaderCode
{
  std::string code;
  std::string location;
  std::string type;
  std::string file;
};
FullShaderCode GetFullShaderCode(std::string file);
std::string CompileShader(std::string fullShaderCode, std::string location, std::string type);
Shader Create(Device::Device device, std::string compiled);
void Destroy(Shader data);
} // namespace Shader
} // namespace Vulkan
} // namespace Graphics
} // namespace Tortuga

#endif