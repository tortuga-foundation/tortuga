using System.Threading.Tasks;
using System.Numerics;
using Tortuga.Audio;

namespace Tortuga.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var engine = new Engine();

            //create new scene
            var scene = new Core.Scene();

            //audio mixer
            var mixer = new MixerGroup();
            mixer.Gain = 2.0f;
            mixer.AddEffect(new Audio.Effect.Echo());

            scene.AddSystem<Audio.AudioSystem>();
            scene.AddSystem<Input.InputSystem>();

            engine.LoadScene(scene);
            await engine.Run();
        }
    }
}
