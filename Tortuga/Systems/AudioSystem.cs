using System;
using System.Threading.Tasks;

namespace Tortuga.Systems
{
    /// <summary>
    /// Audio system is responsible for updating audio listener/sources position, orientation and velocity
    /// </summary>
    public class AudioSystem : Core.BaseSystem
    {
        /// <summary>
        /// Not used
        /// </summary>
        public override void OnDisable()
        {
        }

        /// <summary>
        /// Not used
        /// </summary>
        public override void OnEnable()
        {
            UpdatePositionVelocityAndOrientation();
        }

        /// <summary>
        /// Runs every frame
        /// </summary>
        public override Task Update()
        {
            return Task.Run(() => UpdatePositionVelocityAndOrientation());
        }

        private void UpdatePositionVelocityAndOrientation()
        {
            var audioSource = MyScene.GetComponents<Components.AudioSource>();
            foreach (var source in audioSource)
            {
                if (source.Is3D == false)
                    continue;

                var transform = source.MyEntity.GetComponent<Components.Transform>();
                if (transform != null)
                {
                    source.Velocity = (transform.Position - source.Position) * Time.DeltaTime;
                    source.Position = transform.Position;
                    source.SetOrientation(transform.Up, transform.Forward);
                }
            }
            var audioListener = MyScene.GetComponents<Components.AudioListener>();
            if (audioListener.Length > 1)
                Console.WriteLine("WARN: There must be only 1 audio listener");
            
            if (audioListener.Length != 0)
            {
                var listener = audioListener[0];

                var transform = listener.MyEntity.GetComponent<Components.Transform>();
                if (transform != null)
                {
                    listener.Velocity = (transform.Position - listener.Position) * Time.DeltaTime;
                    listener.Position = transform.Position;
                    listener.SetOrientation(transform.Up, transform.Forward);
                }
            }
        }
    }
}