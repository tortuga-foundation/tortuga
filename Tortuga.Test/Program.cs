using System.Threading.Tasks;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup engine and load required modules
            var engine = new Engine();
            engine.AddModule<Audio.AudioModule>();
            engine.AddModule<Input.InputModule>();

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

            scene.AddSystem<Audio.AudioSystem>();

            engine.LoadScene(scene);
            await engine.Run();
        }
    }
}
