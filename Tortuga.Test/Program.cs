using System.Threading.Tasks;
using System.Numerics;
using Tortuga.Audio;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //setup engine and load required modules
            var engine = new Engine();
            // Setup audio modules
            engine.AddModule<Audio.AudioModule>();
            //setup input module
            engine.AddModule<Input.InputModule>();

            //create new scene
            var scene = new Core.Scene();

            //audio mixer
            var mixer = new MixerGroup();
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
