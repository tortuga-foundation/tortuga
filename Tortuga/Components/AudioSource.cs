using System;
using Tortuga.Audio.API;
using Tortuga.Audio;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;
using System.Numerics;
using System.Collections.Generic;

namespace Tortuga.Components
{
    /// <summary>
    /// Audio source component
    /// </summary>
    public class AudioSource : Core.BaseComponent
    {
        /// <summary>
        /// automatically play's when AudioSystem is initialized
        /// </summary>
        public bool PlayOnEnable;
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
                alSourceiv(_source, ALSource.Buffer, new int[]{ (int)_buffer.Handle });
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
                var val = new int[1];
                alGetSourceiv(_source, ALSource.Looping, val);
                alHandleError("could not get source audio loop: ");
                return val[0] == 1;
            }
            set
            {
                if (value)
                    alSourceiv(_source, ALSource.Looping, new int[]{1});
                else
                    alSourceiv(_source, ALSource.Looping, new int[]{0});
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
                var val = new float[1];
                alGetSourcefv(_source, ALSource.Pitch, val);
                alHandleError("could not get source audio pitch: ");
                return val[0];
            }
            set
            {
                alSourcefv(_source, ALSource.Pitch, new float[]{value});
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
                var val = new float[1];
                alGetSourcefv(_source, ALSource.Gain, val);
                alHandleError("could not get source audio gain: ");
                return val[0];
            }
            set
            {
                alSourcefv(_source, ALSource.Gain, new float[]{value});
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
                int[] val = new int[1];
                alGetSourceiv(_source, ALSource.SourceRelative, val);
                alHandleError("could not get source audio 3D mode: ");
                return val[0] == 1;
            }
            set
            {
                if (value)
                {
                    alSourceiv(_source, ALSource.SourceRelative, new int[]{1});
                    if (_clip != null && _clip.NumberOfChannels > 1)
                        Console.WriteLine("WARN: Audio Clip must be mono (1 channel) for 3D to work");
                }
                else
                    alSourceiv(_source, ALSource.SourceRelative, new int[]{0});
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
                var val = new float[3];
                alGetSourcefv(_source, ALSource.Position, val);
                alHandleError("could not get source audio position: ");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alSourcefv(_source, ALSource.Position, new float[]{ value.X, value.Y, value.Z });
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
                var val = new float[3];
                alGetSourcefv(_source, ALSource.Velocity, val);
                alHandleError("could not get source audio velocity: ");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alSourcefv(_source, ALSource.Velocity, new float[]{ value.X, value.Y, value.Z });
                alHandleError("could not set source velocity: ");
            }
        }

        /// <summary>
        /// The rolloff rate for the source
        /// </summary>
        public float RollOffFactor
        {
            get
            {
                var val = new float[1];
                alGetSourcefv(_source, ALSource.RolloffFactor, val);
                alHandleError("could not get source roll off factor: ");
                return val[0];
            }
            set
            {
                alSourcefv(_source, ALSource.RolloffFactor, new float[]{ value });
                alHandleError("could not set source roll off factor: ");
            }

        }

        /// <summary>
        /// Used to set the distance where there will no longer be any attenuation of the source
        /// </summary>
        public float MaxDistance
        {
            get
            {
                var val = new float[1];
                alGetSourcefv(_source, ALSource.MaxDistance, val);
                alHandleError("could not get source max distance: ");
                return val[0];
            }
            set
            {
                alSourcefv(_source, ALSource.MaxDistance, new float[]{ value });
                alHandleError("could not set source max distance: ");
            }
        }

        /// <summary>
        /// the effects to apply to the audio source
        /// </summary>
        public List<AudioEffect> Effects
        {
            get => _effects;
        }
        private List<AudioEffect> _effects;

        private uint _source;
        private AudioBuffer _buffer;
        private Distortion _efx;

        /// <summary>
        /// Constructor for audio source
        /// </summary>
        public AudioSource()
        {
            alGenSources(out _source);
            alHandleError("failed to generate audio source: ");
            _efx = new Distortion();
            _efx.Edge = 1.0f;
            _efx.Gain = 1.0f;
            alSourceiv(_source, ALSource.AuxiliarySendFilter, new int[]{ (int)_efx.AuxiliarySlot, 0, 0 });
            alHandleError("failed to setup effect on a audio source: ");
            _effects = new List<AudioEffect>();
            this.Is3D = true;
            this.Loop = false;
            this.Gain = 1.0f;
            this.Pitch = 1.0f;
            this.RollOffFactor = 1.0f;
            this.MaxDistance = 100.0f;
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
            alHandleError("failed to destroy source: ");
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
            alHandleError("failed to play source: ");
        }

        /// <summary>
        /// Stop audio source
        /// </summary>
        public void Stop()
        {
            if (_clip == null || _buffer == null)
                return;
            
            alSourceStop(_source);
            alHandleError("failed to stop source: ");
        }

        /// <summary>
        /// Pause audio source playback
        /// </summary>
        public void Pause()
        {
            if (_clip == null || _buffer == null)
                return;
            
            alSourcePause(_source);
            alHandleError("failed to pause source: ");
        }
    
        /// <summary>
        /// Set's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void SetOrientation(Vector3 up, Vector3 forward)
        {
            alSourcefv(_source, ALSource.Orientation, new float[]{
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
            var vals = new float[6];
            alGetSourcefv(_source, ALSource.Orientation, vals);
            forward = new Vector3(vals[0], vals[1], vals[2]);
            up = new Vector3(vals[3], vals[4], vals[5]);
            alHandleError("could not get source audio orientation: ");
        }
    }
}