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
      diff -= 0.1f;
    if (key == Core::Input::KeyCode::Right)
      diff += 0.1f;

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
    t.SetPosition(glm::vec3(0, 0, -7));
    Core::Engine::AddComponent<Components::Transform>(camera, t);
  }

  //setup light
  const auto light = Core::Engine::CreateEntity();
  {
    Components::Transform t;
    t.SetPosition(glm::vec3(3, 5, 3));
    Components::Light l;
    l.SetIntensity(1.0f);
    Core::Engine::AddComponent<Components::Transform>(light, t);
    Core::Engine::AddComponent<Components::Light>(light, l);
  }

  //create a dragon
  //const auto dragon = Core::Engine::CreateEntity();
  //{
  //  const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Dragon.obj");
  //  Core::Engine::AddComponent<Components::Mesh>(dragon, Components::Mesh(mesh));
  //  Components::Transform t;
  //  t.SetScale(glm::vec3(0.5, 0.5, 0.5));
  //  Core::Engine::AddComponent<Components::Transform>(dragon, t);
  //  Components::Material m;
  //  m.SetColor(glm::vec3(1, 0, 0));
  //  Core::Engine::AddComponent<Components::Material>(dragon, m);
  //  Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(dragon);
  //}

  //create a monkey
  const auto monkey = Core::Engine::CreateEntity();
  {
    const auto albedo = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Color.jpg");
    const auto normal = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Normal.jpg");
    const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Monkey.obj");
    Core::Engine::AddComponent<Components::Mesh>(monkey, Components::Mesh(mesh));
    Components::Transform t;
    t.SetPosition(glm::vec3(0, 0, -3));
    t.SetScale(glm::vec3(2, 2, 2));
    Core::Engine::AddComponent<Components::Transform>(monkey, t);
    Components::Material m;
    m.SetColor(glm::vec3(0, 0, 1));
    m.SetAlbedo(Graphics::Image(albedo));
    m.SetNormal(Graphics::Image(normal));
    Core::Engine::AddComponent<Components::Material>(monkey, m);
    Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(monkey);
  }

  //create a sphere
  //const auto sphere = Core::Engine::CreateEntity();
  //{
  //  const auto albedo = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Color.jpg");
  //  const auto normal = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Normal.jpg");
  //  const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Sphere.obj");
  //  Core::Engine::AddComponent<Components::Mesh>(sphere, Components::Mesh(mesh));
  //  Components::Transform t;
  //  t.SetPosition(glm::vec3(0, 0, -3));
  //  t.SetScale(glm::vec3(3, 3, 3));
  //  Core::Engine::AddComponent<Components::Transform>(sphere, t);
  //  Components::Material m;
  //  m.SetColor(glm::vec3(1, 1, 1));
  //  m.SetRoughness(9.);
  //  m.SetMetalic(0);
  //  m.SetAlbedo(Graphics::Image(albedo));
  //  m.SetNormal(Graphics::Image(normal));
  //  Core::Engine::AddComponent<Components::Material>(sphere, m);
  //  Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(sphere);
  //}
 
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