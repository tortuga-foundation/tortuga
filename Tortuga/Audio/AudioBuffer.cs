using System;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio
{
    public class AudioBuffer
    {
        //private ALCdevice _device;
        //private ALCcontext _context;
        //private uint _source;
        private uint _buffer;

        public unsafe AudioBuffer(AudioClip clip)
        {
            /*
            _device = alcOpenDevice(null);
            _context = alcCreateContext(_device);
            alcMakeContextCurrent(_context);
            alListener3f(ALParams.Position, Vector3.Zero);
            Console.WriteLine(alGetError());
            alListener3f(ALParams.Velocity, Vector3.Zero);
            Console.WriteLine(alGetError());

            //setup audio source 
            alGenSources(1, out uint source);
            alSourcef(source, ALParams.Pitch, 1);
            Console.WriteLine(alGetError());
            alSourcef(source, ALParams.Gain, 1);
            Console.WriteLine(alGetError());
            alSource3f(source, ALParams.Position, Vector3.Zero);
            Console.WriteLine(alGetError());
            alSource3f(source, ALParams.Velocity, Vector3.Zero);
            Console.WriteLine(alGetError());
            alSourcei(source, ALParams.Looping, 0);
            Console.WriteLine(alGetError());
            _source = source;
            */
            alGenBuffers(1, out uint buffer);
            alBufferData(
                buffer, 
                GetALFormat(clip.NumberOfChannels, clip.BitsPerSample), 
                new IntPtr(clip.Samples.Data.ToPointer()),
                (int)clip.Samples.Count,
                clip.SampleRate
            );

        }
        ~AudioBuffer()
        {
            //alcDestroyContext(_context);
            //alcCloseDevice(_device);
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

        public unsafe void Play(AudioClip clip)
        {
            //setup buffer
            alGenBuffers(1, out uint buffer);
            Console.WriteLine(alGetError());
            alBufferData(
                buffer, 
                GetALFormat(clip.NumberOfChannels, clip.BitsPerSample), 
                new IntPtr(clip.Samples.Data.ToPointer()),
                (int)clip.Samples.Count, 
                clip.SampleRate
            );
            Console.WriteLine(alGetError());            
            //alSourcei(_source, ALParams.Buffer, (int)buffer);
            Console.WriteLine(alGetError());
            _buffer = buffer;
            //alSourcePlay(_source);
            Console.WriteLine(alGetError());
        }
    }
}