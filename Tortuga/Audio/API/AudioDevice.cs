using System;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.API
{
    internal class AudioDevice
    {
        public ALCdevice Handle => _device;
        private ALCdevice _device;
        public ALCcontext Context => _context;
        private ALCcontext _context;

        public AudioDevice()
        {
            _device = alcOpenDevice(null);
            _context = alcCreateContext(_device);
            alcMakeContextCurrent(_context);
            alHandleError("failed to setup open al context: ");
        }
        ~AudioDevice()
        {
            alcDestroyContext(_context);
            alcCloseDevice(_device);
        }
    }
}