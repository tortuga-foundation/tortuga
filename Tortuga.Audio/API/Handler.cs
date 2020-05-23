

namespace Tortuga.Audio.API
{
    internal static class Handler
    {
        /// <summary>
        /// Audio device handle provided by Open AL
        /// </summary>
        public static AudioDevice Device
        {
            get
            {
                if (_device == null)
                    _device = new AudioDevice();

                return _device;
            }
        }
        private static AudioDevice _device;
    }
}