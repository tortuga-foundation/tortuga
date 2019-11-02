#ifndef _TORTUGA
#define _TORTUGA

//Utils
#include "./Tortuga/Utils/IO.hpp"

//Vulkan library
#include "./Tortuga/Graphics/Vulkan/ErrorCheck.hpp"
#include "./Tortuga/Graphics/Vulkan/Instance.hpp"
#include "./Tortuga/Graphics/Vulkan/Device.hpp"
#include "./Tortuga/Graphics/Vulkan/Swapchain.hpp"
#include "./Tortuga/Graphics/Vulkan/Shader.hpp"
#include "./Tortuga/Graphics/Vulkan/Image.hpp"
#include "./Tortuga/Graphics/Vulkan/Fence.hpp"
#include "./Tortuga/Graphics/Vulkan/Semaphore.hpp"
#include "./Tortuga/Graphics/Vulkan/Buffer.hpp"
#include "./Tortuga/Graphics/Vulkan/CommandPool.hpp"
#include "./Tortuga/Graphics/Vulkan/Command.hpp"
#include "./Tortuga/Graphics/Vulkan/DescriptorLayout.hpp"
#include "./Tortuga/Graphics/Vulkan/DescriptorPool.hpp"
#include "./Tortuga/Graphics/Vulkan/DescriptorSet.hpp"
#include "./Tortuga/Graphics/Vulkan/Pipeline.hpp"
#include "./Tortuga/Graphics/Image.hpp"
#include "./Tortuga/Graphics/DisplaySurface.hpp"

//Core
#include "./Tortuga/Core/Console.hpp"
#include "./Tortuga/Core/GUID.hpp"
#include "./Tortuga/Core/Engine.hpp"

//Components
#include "./Tortuga/Components/Transform.hpp"
#include "./Tortuga/Components/Camera.hpp"
#include "./Tortuga/Components/Mesh.hpp"
#include "./Tortuga/Components/Light.hpp"
#include "./Tortuga/Components/Material.hpp"

//Systems
#include "./Tortuga/Systems/Rendering/Rendering.hpp"

#endif
