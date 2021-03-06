#pragma warning disable 1591
using System;
using System.Threading.Tasks;

namespace Tortuga.Audio
{
    /// <summary>
    /// Audio system is responsible for updating audio listener/sources position, orientation and velocity
    /// </summary>
    public class AudioSystem : Core.BaseSystem
    {
        /// <summary>
        /// Called when system is disabled
        /// </summary>
        public override void OnDisable()
        {
            var audioSource = MyScene.GetComponents<AudioSource>();
            foreach (var source in audioSource)
                source.Stop();
        }

        /// <summary>
        /// Called when system is activated
        /// </summary>
        public override void OnEnable()
        {
            UpdatePositionVelocityAndOrientation();
            var audioSource = MyScene.GetComponents<AudioSource>();
            foreach (var source in audioSource)
                source.Play();
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
            var audioSource = MyScene.GetComponents<AudioSource>();
            foreach (var source in audioSource)
            {
                if (source.Is3D == false)
                    continue;

                var transform = source.MyEntity.GetComponent<Core.Transform>();
                if (transform != null)
                {
                    source.Velocity = (transform.Position - source.Position) * Time.DeltaTime;
                    source.Position = transform.Position;
                    source.SetOrientation(transform.Up, transform.Forward);
                }
            }
            var audioListener = MyScene.GetComponents<AudioListener>();
            if (audioListener.Length > 1)
                Console.WriteLine("WARN: There must be only 1 audio listener");

            if (audioListener.Length != 0)
            {
                var listener = audioListener[0];

                var transform = listener.MyEntity.GetComponent<Core.Transform>();
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