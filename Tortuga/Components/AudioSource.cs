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
        /// If true audio source will use position, velocity and orientation
        /// </summary>
        public bool Is3D
        {
            get
            {
                alGetSourcei(_source, ALParams.SourceRelative, out int val);
                return val == 1;
            }
            set
            {
                if (value)
                    alSourcei(_source, ALParams.SourceRelative, 1);
                else
                    alSourcei(_source, ALParams.SourceRelative, 0);
            }
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
            this.SetOrientation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
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
    
        /// <summary>
        /// Set's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void SetOrientation(Vector3 up, Vector3 forward)
        {
            alSourcefv(_source, ALParams.Orientation, new float[]{
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z 
            });
        }

        /// <summary>
        /// Get's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void GetOrientation(out Vector3 up, out Vector3 forward)
        {
            alGetSourcefv(_source, ALParams.Orientation, out float[] vals);
            forward = new Vector3(vals[0], vals[1], vals[2]);
            up = new Vector3(vals[3], vals[4], vals[5]);
        }
    }
}