using System.Threading.Tasks;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup open al audio
            Engine.Instance.AddModule<Audio.AudioModule>();
            //setup sdl input system
            Engine.Instance.AddModule<Input.InputModule>();
            //setup vulkan instance
            Engine.Instance.AddModule<Graphics.GraphicsModule>();

            //create new scene
            var scene = new Core.Scene();

            //audio mixer
            var mixer = new Audio.MixerGroup();
            mixer.Gain = 2.0f;
            mixer.AddEffect(new Audio.Effect.Echo());

            Input.InputModule.OnKeyDown += (Input.KeyCode key, Input.ModifierKeys modifiers) =>
            {
                System.Console.WriteLine(key.ToString());
            };

            var window = new Graphics.Window(
                "Tortuga",
                0, 0,
                1920, 1080,
                Graphics.WindowType.Window
            );
            Input.InputModule.OnWindowClose += (uint windowId) =>
            {
                if (window.WindowIdentifier == windowId)
                    Engine.Instance.IsRunning = false;
            };
            Input.InputModule.OnApplicationClose += () => Engine.Instance.IsRunning = false;

            //entity
            {
                var entity = new Core.Entity();
                var listner = await entity.AddComponent<Audio.AudioListener>();
                var source = await entity.AddComponent<Audio.AudioSource>();
                source.Output = mixer;
                source.Clip = await Audio.AudioClip.Load("Assets/Audio/pcm mono 16 bit 32kHz.wav");
                source.Loop = true;
                var camera = await entity.AddComponent<Graphics.Camera>();
                camera.RenderToWindow = window;
                scene.AddEntity(entity);
            }

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Graphics.RenderingSystem>();

            Engine.Instance.LoadScene(scene);
            await Engine.Instance.Run();
        }
    }
}
