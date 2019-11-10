#include "./DisplaySurface.hpp"
#include <unordered_map>

namespace Tortuga
{
namespace Graphics
{
namespace DisplaySurface
{
DisplaySurface Create(Vulkan::Instance::Instance instance, Vulkan::Device::Device device)
{
  const uint32_t defaultWidth = 1920;
  const uint32_t defaultHeight = 1080;

  auto data = DisplaySurface();
  data.Instance = instance.Instance;
  data.Window = SDL_CreateWindow("Tortuga", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, defaultWidth, defaultHeight, SDL_WINDOW_VULKAN | SDL_WINDOW_RESIZABLE);
  if (SDL_Vulkan_CreateSurface(data.Window, instance.Instance, &data.Surface) != SDL_TRUE)
  {
    Console::Error("SDL: Failed to create vulkan surface");
    return data;
  }
  data.Swapchain = Vulkan::Swapchain::Create(device, defaultWidth, defaultHeight, data.Surface);

  return data;
}
void Destroy(DisplaySurface data)
{
  Vulkan::Swapchain::Destroy(data.Swapchain);
  vkDestroySurfaceKHR(data.Instance, data.Surface, nullptr);
  SDL_DestroyWindow(data.Window);
}
void SetTitle(DisplaySurface data, std::string title)
{
  SDL_SetWindowTitle(data.Window, title.c_str());
}

std::unordered_map<uint32_t, bool> keysDown;
bool isMaximized = false;
void Dispatch(DisplaySurface data)
{
  SDL_Event event;
  if (SDL_PollEvent(&event) != 0)
  {
    //window events
    if (event.window.event == SDL_WINDOWEVENT_CLOSE)
      Core::Input::TriggerWindowClose();
    if (event.window.event == SDL_WINDOWEVENT_FOCUS_GAINED)
      Core::Input::TriggerWindowFocus(true);
    if (event.window.event == SDL_WINDOWEVENT_FOCUS_LOST)
      Core::Input::TriggerWindowFocus(false);
    if (event.window.event == SDL_WINDOWEVENT_RESIZED)
      Core::Input::TriggerWindowResize(event.window.data1, event.window.data2);
    if (event.window.event == SDL_WINDOWEVENT_MINIMIZED)
      Core::Input::TriggerWindowMinimize(true);
    if (event.window.event == SDL_WINDOWEVENT_RESTORED)
      Core::Input::TriggerWindowMinimize(false);
    if (event.window.event == SDL_WINDOWEVENT_MAXIMIZED)
    {
      isMaximized = true;
      Core::Input::TriggerWindowMaximized(true);
    }
    if (event.window.event == SDL_WINDOWEVENT_SIZE_CHANGED)
    {
      if (isMaximized)
      {
        isMaximized = false;
        Core::Input::TriggerWindowMaximized(false);
      }
    }
    //keyboard events
    if (event.type == SDL_KEYDOWN && !keysDown[event.key.keysym.sym])
    {
      if (event.key.keysym.sym != 0)
      {
        Core::Input::TriggerKeyEvent((Core::Input::KeyCode)event.key.keysym.sym, Core::Input::KeyAction::Pressed);
        keysDown[event.key.keysym.sym] = true;
      }
    }
    if (event.type == SDL_KEYUP)
    {
      if (event.key.keysym.sym != 0)
      {
        Core::Input::TriggerKeyEvent((Core::Input::KeyCode)event.key.keysym.sym, Core::Input::KeyAction::Released);
        keysDown[event.key.keysym.sym] = false;
      }
    }
    //mouse events
    if (event.type == SDL_MOUSEMOTION)
      Core::Input::TriggerMouseEvent(event.motion.x, event.motion.y);
    if (event.type == SDL_MOUSEBUTTONDOWN && event.button.button < 9)
    {
      Core::Input::TriggerKeyEvent((Core::Input::KeyCode)event.button.button, Core::Input::KeyAction::Pressed);
      keysDown[event.button.button] = true;
    }
    if (event.type == SDL_MOUSEBUTTONUP && event.button.button < 9)
    {
      Core::Input::TriggerKeyEvent((Core::Input::KeyCode)event.button.button, Core::Input::KeyAction::Released);
      keysDown[event.button.button] = false;
    }
    if (event.type == SDL_MOUSEWHEEL)
      Core::Input::TriggerScrollEvent(event.wheel.x, event.wheel.y);
    //other events
    if (event.type == SDL_DROPFILE)
      Core::Input::TriggerFileDrop({event.drop.file});
  }

  for (const auto k : keysDown)
    if (k.second)
      Core::Input::TriggerKeyEvent((Core::Input::KeyCode)k.first, Core::Input::KeyAction::HeldDown);
}
} // namespace DisplaySurface
} // namespace Graphics
} // namespace Tortuga