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
                alHandleError("could not set source audio clip: ");
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
                alHandleError("could not get source audio loop: ");
                return val == 1;
            }
            set
            {
                if (value)
                    alSourcei(_source, ALParams.Looping, 1);
                else
                    alSourcei(_source, ALParams.Looping, 0);
                alHandleError("could not set source loop: ");
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
                alHandleError("could not get source audio pitch: ");
                return val;
            }
            set
            {
                alSourcef(_source, ALParams.Pitch, value);
                alHandleError("could not set source pitch: ");
            }
        }
        /// <summary>
        /// Audio source gain
        /// </summary>
        public float Gain
        {
            get
            {
                alGetSourcef(_source, ALParams.Gain, out float val);
                alHandleError("could not get source audio gain: ");
                return val;
            }
            set
            {
                alSourcef(_source, ALParams.Gain, value);
                alHandleError("could not set source gain: ");
            }
        }

        /// <summary>
        /// If true audio source will use position, velocity and orientation
        /// </summary>
        public bool Is3D
        {
            get
            {
                alGetSourcei(_source, ALParams.SourceRelative, out int val);
                alHandleError("could not get source audio 3D mode: ");
                if (_clip.NumberOfChannels > 1)
                    Console.WriteLine("WARN: Audio Clip must be mono (1 channel) for 3D to work");
                return val == 1;
            }
            set
            {
                if (value)
                    alSourcei(_source, ALParams.SourceRelative, 1);
                else
                    alSourcei(_source, ALParams.SourceRelative, 0);
                alHandleError("could not set source 3D mode: ");
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
                alHandleError("could not get source audio position: ");
                return new Vector3(x, y, z);
            }
            set
            {
                alSource3f(_source, ALParams.Position, value.X, value.Y, value.Z);
                alHandleError("could not set source position: ");
            }
        }
        /// <summary>
        /// Audio source velocity
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                alGetSource3f(_source, ALParams.Velocity, out float x, out float y, out float z);
                alHandleError("could not get source audio velocity: ");
                return new Vector3(x, y, z);
            }
            set
            {
                alSource3f(_source, ALParams.Velocity, value.X, value.Y, value.Z);
                alHandleError("could not set source velocity: ");
            }
        }

        private uint _source;
        private AudioBuffer _buffer;

        /// <summary>
        /// Constructor for audio source
        /// </summary>
        public AudioSource()
        {
            alGenSources(out _source);
            alHandleError("failed to generate audio source: ");
            this.Is3D = true;
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
            alHandleError("could not set source audio orientation: ");
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
            alHandleError("could not get source audio orientation: ");
        }
    }
}