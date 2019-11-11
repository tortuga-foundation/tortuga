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

  //create a sphere
  const auto sphere = Core::Engine::CreateEntity();
  {
    const auto albedo = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Color.jpg");
    const auto normal = Utils::IO::LoadImageFile("Assets/Textures/Bricks/Normal.jpg");
    const auto specular = Graphics::Image(Utils::IO::LoadImageFile("Assets/Textures/Bricks/Gloss.jpg"));
    const auto reflection = Graphics::Image(Utils::IO::LoadImageFile("Assets/Textures/Bricks/Reflection.jpg"));
    const auto ambientOcclusion = Graphics::Image(Utils::IO::LoadImageFile("Assets/Textures/Bricks/AmbientOcclusion.jpg"));
    
    //copy specular, reflection and ambient occlusion to 1 image texture
    auto detailTexture = Graphics::Image(specular.Width, specular.Height);
    detailTexture.CopyChannel(specular, Graphics::Image::CHANNEL_R, Graphics::Image::CHANNEL_R);
    detailTexture.CopyChannel(reflection, Graphics::Image::CHANNEL_R, Graphics::Image::CHANNEL_G);
    detailTexture.CopyChannel(ambientOcclusion, Graphics::Image::CHANNEL_R, Graphics::Image::CHANNEL_B);

    const auto mesh = Utils::IO::LoadObjFile("Assets/Models/Sphere.obj");
    Core::Engine::AddComponent<Components::Mesh>(sphere, Components::Mesh(mesh));
    Components::Transform t;
    t.SetPosition(glm::vec3(0, 0, -3));
    t.SetScale(glm::vec3(3, 3, 3));
    Core::Engine::AddComponent<Components::Transform>(sphere, t);
    Components::Material m;
    m.SetColor(glm::vec3(1, 1, 1));
    m.SetRoughness(9.);
    m.SetMetalic(0);
    m.SetAlbedo(Graphics::Image(albedo));
    m.SetNormal(Graphics::Image(normal));
    Core::Engine::AddComponent<Components::Material>(sphere, m);
    Core::Engine::AddComponent<ModelRotationSystem::RotationComponent>(sphere);
  }

  //add a rendering system to the engine
  Core::Engine::AddSystem<Systems::Rendering>();
  //add a rotation system which can rorate the model
  Core::Engine::AddSystem<ModelRotationSystem>();

  //main loop
  while (!ShouldClose)
    Core::Engine::IterateSystems();

  //auto destroys everything
  Core::Engine::Destroy();
  return EXIT_SUCCESS;
}