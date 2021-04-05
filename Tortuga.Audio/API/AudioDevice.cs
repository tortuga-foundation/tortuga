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
            _context = alcCreateContext(_device, null);
            alcMakeContextCurrent(_context);
            alHandleError("failed to setup open al context: ");
            if (alcIsExtensionPresent(_device, "ALC_EXT_EFX") == false)
                throw new NotSupportedException("your system does not support open al efx extension");
            alHandleError("failed to setup open al: ");
            UpdateDistanceModel();
            UpdateSpeedOfSound();
        }
        ~AudioDevice()
        {
            alcDestroyContext(_context);
            alcCloseDevice(_device);
        }

        public void UpdateDoplerFactor()
        {
            alDopplerFactor(Settings.DoplerFactor);
            alHandleError("failed to update dopler factor");
        }

        public void UpdateSpeedOfSound()
        {
            alSpeedOfSound(Settings.SpeedOfSound);
            alHandleError("failed to update speed of sound");
        }

        public void UpdateDistanceModel()
        {
            if (Settings.DistanceModel == AudioDistanceModel.LinearDistance)
            {
                if (Settings.IsDistanceModelClamped)
                    alDistanceModel(ALDistanceModel.LinearDistanceClamped);
                else
                    alDistanceModel(ALDistanceModel.LinearDistance);
            }
            else if (Settings.DistanceModel == AudioDistanceModel.ExponentDistance)
            {
                if (Settings.IsDistanceModelClamped)
                    alDistanceModel(ALDistanceModel.ExponentDistanceClamped);
                else
                    alDistanceModel(ALDistanceModel.ExponentDistance);
            }
            else if (Settings.DistanceModel == AudioDistanceModel.InverseDistance)
            {
                if (Settings.IsDistanceModelClamped)
                    alDistanceModel(ALDistanceModel.InverseDistanceClamped);
                else
                    alDistanceModel(ALDistanceModel.InverseDistance);
            }
            else
                alDistanceModel(ALDistanceModel.None);
            alHandleError("failed to assign distance model: ");
        }
    }
}