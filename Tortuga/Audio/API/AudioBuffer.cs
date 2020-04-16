using System;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.API
{
    /// <summary>
    /// Open AL Audio Buffer object
    /// </summary>
    internal class AudioBuffer
    {
        public uint Handle => _buffer;
        private uint _buffer;

        /// <summary>
        /// Constructor for Audio Buffer
        /// </summary>
        /// <param name="clip">Audio clip to use</param>
        public unsafe AudioBuffer(AudioClip clip)
        {
            alGenBuffers(out uint buffer);
            _buffer = buffer;
            alBufferData(
                _buffer, 
                GetALFormat(clip.NumberOfChannels, clip.BitsPerSample), 
                new IntPtr(clip.Samples.Data.ToPointer()),
                (int)clip.Samples.Count,
                clip.SampleRate
            );
            var error = alGetError();
            if (error != ALError.None)
                throw new FormatException("Audio clip is not formatted correctly");
        }
        /// <summary>
        /// De-Constructor for AudioBuffer
        /// </summary> 
        ~AudioBuffer()
        {
            alDeleteBuffers(1, new uint[]{ _buffer });
        }

        private ALFormat GetALFormat(int channels, int bitsPerSample)
        {
            bool stereo = channels > 1;

            if (bitsPerSample == 16)
            {
                if (channels > 1)
                    return ALFormat.Stereo16;
                else
                    return ALFormat.Mono16;
            }
            else if (bitsPerSample == 8)
            {
                if (channels > 1)
                    return ALFormat.Stereo8;
                else
                    return ALFormat.Mono8;
            }

            throw new Exception("Invalid audio clip format");
        }
    }
}