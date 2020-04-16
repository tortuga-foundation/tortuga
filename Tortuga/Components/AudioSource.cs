using System;
using Tortuga.Audio.API;
using Tortuga.Audio;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;
using System.Numerics;

namespace Tortuga.Components
{
    /// <summary>
    /// Audio source component
    /// </summary>
    public class AudioSource : Core.BaseComponent
    {
        /// <summary>
        /// Audio clip used by the source
        /// </summary>
        public AudioClip Clip
        {
            get => _clip;
            set
            {
                _clip = value;
                _buffer = new AudioBuffer(_clip);
                alSourcei(_source, ALParams.Buffer, (int)_buffer.Handle);
            }
        }
        private AudioClip _clip;
        /// <summary>
        /// Loop the audio clip
        /// </summary>
        public bool Loop
        {
            get
            {
                alGetSourcei(_source, ALParams.Looping, out int val);
                return val == 1;
            }
            set
            {
                if (value)
                    alSourcei(_source, ALParams.Looping, 1);
                else
                    alSourcei(_source, ALParams.Looping, 0);
            }
        }
        /// <summary>
        /// Audio source pitch
        /// </summary>
        public float Pitch
        {
            get
            {
                alGetSourcef(_source, ALParams.Pitch, out float val);
                return val;
            }
            set => alSourcef(_source, ALParams.Pitch, value);
        }
        /// <summary>
        /// Audio source gain
        /// </summary>
        public float Gain
        {
            get
            {
                alGetSourcef(_source, ALParams.Gain, out float val);
                return val;
            }
            set => alSourcef(_source, ALParams.Gain, value);
        }

        /// <summary>
        /// Audio source position
        /// </summary>
        public Vector3 Position
        {
            get
            {
                alGetSource3f(_source, ALParams.Position, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set => alSource3f(_source, ALParams.Position, value);
        }
        /// <summary>
        /// Audio source velocity
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                alGetSource3f(_source, ALParams.Velocity, out float x, out float y, out float z);
                return new Vector3(x, y, z);
            }
            set => alSource3f(_source, ALParams.Velocity, value);
        }


        private uint _source;
        private AudioBuffer _buffer;

        /// <summary>
        /// Constructor for audio source
        /// </summary>
        public AudioSource()
        {
            alGenSources(out _source);
            this.Loop = false;
            this.Gain = 1.0f;
            this.Pitch = 1.0f;
            this.Position = Vector3.Zero;
            this.Velocity = Vector3.Zero;
        }
        /// <summary>
        /// De-Constructor for audio source
        /// </summary>
        ~AudioSource()
        {
            alDeleteSources(1, new uint[]{ _source });
        }

        /// <summary>
        /// Play the audio source
        /// </summary>
        public void Play()
        {
            if (_clip == null || _buffer == null)
            {
                Console.WriteLine("no audio clip assigned");
                return;
            }
            alSourcePlay(_source);
        }
    }
}