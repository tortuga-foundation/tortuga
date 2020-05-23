using System;
using Tortuga.Audio.Effect;
using Tortuga.Audio.API;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tortuga.Audio
{
    /// <summary>
    /// Audio source component
    /// </summary>
    public class AudioSource : Core.BaseComponent
    {
        /// <summary>
        /// This can be used to re route the output of this audio source to a mixer
        /// </summary>
        public MixerGroup Output
        {
            get => _output;
            set
            {
                if (_output != null)
                {
                    _output.OnMixerEffectsUpdated -= OnMixerEffectsUpdated;
                    _output.OnMixerSettingsUpdated -= OnMixerSetttingsUpdated;
                }
                _output = value;
                if (_output == null)
                    return;
                _output.OnMixerEffectsUpdated += OnMixerEffectsUpdated;
                _output.OnMixerSettingsUpdated += OnMixerSetttingsUpdated;
                OnMixerEffectsUpdated();
                OnMixerSetttingsUpdated();
            }
        }
        private MixerGroup _output;

        private void OnMixerEffectsUpdated()
        {
            if (_output == null)
                return;

            var maxSlots = new int[1];
            alcGetIntegerv(API.Handler.Device.Handle, (int)ALAuxiliaryEffectSlot.MaxSends, maxSlots);
            alHandleError("failed to get maximum effect slots allowed: ");
            var effects = _output.FullEffects.ToArray();
            for (int i = 0; i < effects.Length; i++)
            {
                int filter = (int)ALFilter.None;
                if (effects[i].Filter != null)
                    filter = (int)effects[i].Filter.Handle;

                alSourceiv(_handle, ALSource.AuxiliarySendFilter, new int[]{ (int)effects[i].AuxiliarySlot, i, filter });
                alHandleError("failed to remove effect from audio source: ");
            }
            for (int i = effects.Length; i < maxSlots[0]; i++)
            {
                alSourceiv(_handle, ALSource.AuxiliarySendFilter, new int[]{ (int)ALAuxiliaryEffectSlot.None, i, (int)ALFilter.None });
                alHandleError("failed to remove effect from audio source: ");
            }
        }
        private void OnMixerSetttingsUpdated()
        {
            if (_output == null)
                return;
            
            alSourcefv(_handle, ALSource.Pitch, new float[]{ _output.FullPitch * _pitch });
            alHandleError("could not set source pitch: ");
            alSourcefv(_handle, ALSource.Gain, new float[]{ _output.FullGain * _gain });
            alHandleError("could not set source gain: ");
        }

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
                alSourceiv(_handle, ALSource.Buffer, new int[]{ (int)_buffer.Handle });
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
                alGetSourceiv(_handle, ALSource.Looping, val);
                alHandleError("could not get source audio loop: ");
                return val[0] == 1;
            }
            set
            {
                if (value)
                    alSourceiv(_handle, ALSource.Looping, new int[]{1});
                else
                    alSourceiv(_handle, ALSource.Looping, new int[]{0});
                alHandleError("could not set source loop: ");
            }
        }
        /// <summary>
        /// Audio source pitch
        /// </summary>
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                float val = value;
                if (_output != null)
                    val = _output.FullPitch * _pitch;

                alSourcefv(_handle, ALSource.Pitch, new float[]{ val });
                alHandleError("could not set source pitch: ");
            }
        }
        private float _pitch;
        /// <summary>
        /// Audio source gain
        /// </summary>
        public float Gain
        {
            get => _gain;
            set
            {
                _gain = value;
                float val = value;
                if (_output != null)
                    val = _output.FullGain * _gain;

                alSourcefv(_handle, ALSource.Gain, new float[]{ val });
                alHandleError("could not set source gain: ");
            }
        }
        private float _gain;

        /// <summary>
        /// If true audio source will use position, velocity and orientation
        /// </summary>
        public bool Is3D
        {
            get
            {
                int[] val = new int[1];
                alGetSourceiv(_handle, ALSource.SourceRelative, val);
                alHandleError("could not get source audio 3D mode: ");
                return val[0] == 1;
            }
            set
            {
                if (value)
                {
                    alSourceiv(_handle, ALSource.SourceRelative, new int[]{1});
                    if (_clip != null && _clip.NumberOfChannels > 1)
                        Console.WriteLine("WARN: Audio Clip must be mono (1 channel) for 3D to work");
                }
                else
                    alSourceiv(_handle, ALSource.SourceRelative, new int[]{0});
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
                alGetSourcefv(_handle, ALSource.Position, val);
                alHandleError("could not get source audio position: ");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alSourcefv(_handle, ALSource.Position, new float[]{ value.X, value.Y, value.Z });
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
                alGetSourcefv(_handle, ALSource.Velocity, val);
                alHandleError("could not get source audio velocity: ");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alSourcefv(_handle, ALSource.Velocity, new float[]{ value.X, value.Y, value.Z });
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
                alGetSourcefv(_handle, ALSource.RolloffFactor, val);
                alHandleError("could not get source roll off factor: ");
                return val[0];
            }
            set
            {
                alSourcefv(_handle, ALSource.RolloffFactor, new float[]{ value });
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
                alGetSourcefv(_handle, ALSource.MaxDistance, val);
                alHandleError("could not get source max distance: ");
                return val[0];
            }
            set
            {
                alSourcefv(_handle, ALSource.MaxDistance, new float[]{ value });
                alHandleError("could not set source max distance: ");
            }
        }

        /// <summary>
        /// the effects to apply to the audio source, use AddEffect and RemoveEffect functions to add or remove audio effects
        /// </summary>
        public AudioEffect[] Effects => _effects.ToArray();
        private List<AudioEffect> _effects;

        internal uint Handle => _handle;
        private uint _handle;
        private AudioBuffer _buffer;

        /// <summary>
        /// Constructor for audio source
        /// </summary>
        public AudioSource()
        {
            _effects = new List<AudioEffect>();
            alGenSources(out _handle);
            alHandleError("failed to generate audio source: ");
            this.Is3D = true;
            this.Loop = false;
            this.Gain = 1.0f;
            this.Pitch = 1.0f;
            this.RollOffFactor = 1.0f;
            this.MaxDistance = 100.0f;
            this.Position = Vector3.Zero;
            this.Velocity = Vector3.Zero;
            this.SetOrientation(new Vector3(0, 1, 0), new Vector3(0, 0, 1));
            this.Output = null;
        }
        /// <summary>
        /// De-Constructor for audio source
        /// </summary>
        ~AudioSource()
        {
            alDeleteSources(1, new uint[]{ _handle });
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
            alSourcePlay(_handle);
            alHandleError("failed to play source: ");
        }

        /// <summary>
        /// Stop audio source
        /// </summary>
        public void Stop()
        {
            if (_clip == null || _buffer == null)
                return;
            
            alSourceStop(_handle);
            alHandleError("failed to stop source: ");
        }

        /// <summary>
        /// Pause audio source playback
        /// </summary>
        public void Pause()
        {
            if (_clip == null || _buffer == null)
                return;
            
            alSourcePause(_handle);
            alHandleError("failed to pause source: ");
        }
    
        /// <summary>
        /// Set's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void SetOrientation(Vector3 up, Vector3 forward)
        {
            alSourcefv(_handle, ALSource.Orientation, new float[]{
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
            alGetSourcefv(_handle, ALSource.Orientation, vals);
            forward = new Vector3(vals[0], vals[1], vals[2]);
            up = new Vector3(vals[3], vals[4], vals[5]);
            alHandleError("could not get source audio orientation: ");
        }

        /// <summary>
        /// every frame check if effect is dirty and needs reloading
        /// </summary>
        public override Task Update()
        {
            return Task.Run(() => 
            {
                for (int i = 0; i < _effects.Count; i++)
                {
                    var effect = _effects[i];
                    if (effect.IsDirty == false)
                        continue;
                    
                    int filterHandle = (int)ALFilter.None;
                    if (effect.Filter != null)
                        filterHandle = (int)effect.Filter.Handle;
                    alSourceiv(_handle, ALSource.AuxiliarySendFilter, new int[]{ (int)effect.AuxiliarySlot, i, filterHandle });
                    alHandleError("failed to attach effect to audio source: ");
                }
            });
        }
    }
}