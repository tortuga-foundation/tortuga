#include "./Tortuga.hpp"

using namespace Tortuga;
bool ShouldClose = false;

//mesh rotation system
struct ModelRotationSystem : Core::ECS::System
{
  struct RotationComponent : Core::ECS::Component
  {
    float Rotation;
  };

  static void OnKeyEvent(Core::Input::KeyCode key, Core::Input::KeyAction action)
  {
    if (key != Core::Input::KeyCode::Left && key != Core::Input::KeyCode::Right)
      return;

    float diff = 0.0f;
    if (key == Core::Input::KeyCode::Left)
      diff -= 0.001f;
    if (key == Core::Input::KeyCode::Right)
      diff += 0.001f;

    const auto comps = Core::Engine::GetComponents<RotationComponent>();
    for (const auto comp : comps)
    {
      comp->Rotation += diff;
      const auto transform = Core::Engine::GetComponent<Components::Transform>(comp->Root);
      if (transform != nullptr)
        transform->SetRotation(glm::vec4(0, comp->Rotation, 0, 1));
    }
  }

  ModelRotationSystem()
  {
    Core::Input::NotifyOnKeyEvent(OnKeyEvent);
  }
  ~ModelRotationSystem()
  {
    Core::Input::RemoveOnKeyEvent(OnKeyEvent);
  }
};

int main()
{
  //setup engine
  Core::Engine::Create();

  //setup window close event
  Core::Input::NotifyOnWindowClose([] {
    ShouldClose = true;
  });

  //setup camera
  const auto camera = Core::Engine::CreateEntity();
  {
    Core::Engine::AddComponent<Components::Camera>(camera);
    Components::Transform t;
    t.SetPosition(glm::vec3(0, 2, -7));
    Core::Engine::AddComponent<Components::Transform>(camera, t);
  }

  //setup light
  const auto light = Core::Engine::CreateEntity();
  {
    Components::Transform t;
    t.SetPosition(glm::vec3(3, 5, 3));
    Core::Engine::AddComponent<Components::Transform>(light, t);
    Core::Engine::AddComponent<Components::Light>(light);
  }

  //create a dragon
  const auto dragon = Core::Engine::CreateEntity();
  {
    const auto albedo = Utils::IO::LoadImageFile("Assets/Textures/Sample.jpg");
    const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Dragon.obj");
    Core::Engine::AddComponent<Components::Mesh>(dragon, Components::Mesh(mesh));
    Components::Transform t;
    t.SetScale(glm::vec3(0.5, 0.5, 0.5));
    Core::Engine::AddComponent<Components::Transform>(dragon, t);
    Components::Material m;
    m.SetColor(glm::vec3(1, 0, 0));
    m.SetAlbedo(Graphics::Image(albedo));
    Core::Engine::AddComponent<Components::Material>(dragon, m);
    Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(dragon);
  }

  //create a monkey
  const auto monkey = Core::Engine::CreateEntity();
  {
    const auto albedo = Utils::IO::LoadImageFile("Assets/Textures/Sample.jpg");
    const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Monkey.obj");
    Core::Engine::AddComponent<Components::Mesh>(monkey, Components::Mesh(mesh));
    Components::Transform t;
    t.SetPosition(glm::vec3(0, 0, -3));
    t.SetScale(glm::vec3(2, 2, 2));
    Core::Engine::AddComponent<Components::Transform>(monkey, t);
    Components::Material m;
    m.SetColor(glm::vec3(0, 0, 1));
    m.SetAlbedo(Graphics::Image(albedo));
    Core::Engine::AddComponent<Components::Material>(monkey, m);
    Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(monkey);
  }

  //add a rendering system to the engine
  Core::Engine::AddSystem<Systems::Rendering>();
  //add a rotation system which can rorate the model
  Core::Engine::AddSystem<ModelRotationSystem>();

  //main loop
  while (!ShouldClose)
    Core::Engine::IterateSystems();
  //make sure we are done rendering before destroying everything
  Core::Engine::GetSystem<Systems::Rendering>()->WaitForDevice();

  //auto destroys everything
  Core::Engine::Destroy();
  return EXIT_SUCCESS;
}