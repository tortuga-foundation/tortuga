#pragma warning disable 1591

namespace Tortuga.Audio
{
    /// <summary>
    /// Audio module 
    /// </summary>
    public class AudioModule : Core.BaseModule
    {
        public override void Destroy()
        {

        }

        public override void Init()
        {
            API.Handler.Init();
        }

        public override void Update()
        {

        }
    }
}