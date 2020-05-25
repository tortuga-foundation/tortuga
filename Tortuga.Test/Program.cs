using System.Threading.Tasks;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup engine and load required modules
            Engine.Instance.AddModule<Audio.AudioModule>();
            Engine.Instance.AddModule<Input.InputModule>();
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
