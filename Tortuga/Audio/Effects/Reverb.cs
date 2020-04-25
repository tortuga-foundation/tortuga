
using System.Numerics;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// Reverb audio effect
    /// </summary>
    public class Reverb : AudioEffect
    {
        private bool _isEAX;

        /// <summary>
        /// reverb decay time
        /// </summary>
        public float DecayTime
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.DecayTime, val);
                else
                    alGetEffectfv(_effect, ALReverb.DecayTime, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.DecayTime, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.DecayTime, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb diffusion
        /// </summary>
        public float Diffusion
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.Diffusion, val);
                else
                    alGetEffectfv(_effect, ALReverb.Diffusion, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.Diffusion, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.Diffusion, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb gain
        /// </summary>
        public float Gain
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.Gain, val);
                else
                    alGetEffectfv(_effect, ALReverb.Gain, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.Gain, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.Gain, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb gain HF
        /// </summary>
        public float GainHF
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.GainHF, val);
                else
                    alGetEffectfv(_effect, ALReverb.GainHF, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.GainHF, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.GainHF, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb decay HF ratio
        /// </summary>
        public float DecayHFRatio
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.DecayHFRatio, val);
                else
                    alGetEffectfv(_effect, ALReverb.DecayHFRatio, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.DecayHFRatio, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.DecayHFRatio, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb reflections delay
        /// </summary>
        public float ReflectionsDelay
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.ReflectionsDelay, val);
                else
                    alGetEffectfv(_effect, ALReverb.ReflectionsDelay, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.ReflectionsDelay, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.ReflectionsDelay, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb reflections gain
        /// </summary>
        public float ReflectionsGain
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.ReflectionsGain, val);
                else
                    alGetEffectfv(_effect, ALReverb.ReflectionsGain, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.ReflectionsGain, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.ReflectionsGain, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }



        /// <summary>
        /// reverb late reverb gain
        /// </summary>
        public float LateReverbGain
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.LateReverbGain, val);
                else
                    alGetEffectfv(_effect, ALReverb.LateReverbGain, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.LateReverbGain, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.LateReverbGain, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb late reverb delay
        /// </summary>
        public float LateReverbDelay
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.LateReverbDelay, val);
                else
                    alGetEffectfv(_effect, ALReverb.LateReverbDelay, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.LateReverbDelay, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.LateReverbDelay, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb air absorption gain hf
        /// </summary>
        public float AirAbsorptionGainHF
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.AirAbsorptionGainHF, val);
                else
                    alGetEffectfv(_effect, ALReverb.AirAbsorptionGainHF, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.AirAbsorptionGainHF, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.AirAbsorptionGainHF, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb room roll off factor
        /// </summary>
        public float RoomRollOffFactor
        {
            get
            {
                var val = new float[1];
                if (_isEAX)
                    alGetEffectfv(_effect, ALReverbEAX.RoomRollOffFactor, val);
                else
                    alGetEffectfv(_effect, ALReverb.RoomRollOffFactor, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX)
                    alEffectfv(_effect, ALReverbEAX.RoomRollOffFactor, new float[]{ value });
                else
                    alEffectfv(_effect, ALReverb.RoomRollOffFactor, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb decay hf limit
        /// </summary>
        public bool DecayHFLimit
        {
            get
            {
                var val = new int[1];
                if (_isEAX)
                    alGetEffectiv(_effect, ALReverbEAX.DecayHFLimit, val);
                else
                    alGetEffectiv(_effect, ALReverb.DecayHFLimit, val);
                alHandleError("failed to get reverb variable");
                return val[0] == 1;
            }
            set
            {
                int val = 0;
                if (value)
                    val = 1;

                if (_isEAX)
                    alEffectiv(_effect, ALReverbEAX.DecayHFLimit, new int[]{ val });
                else
                    alEffectiv(_effect, ALReverb.DecayHFLimit, new int[]{ val });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb gain LF
        /// </summary>
        public float GainLF
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.GainLF, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.GainLF, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb decay LF ratio
        /// </summary>
        public float DecayLFRatio
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.DecayLFRatio, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.DecayLFRatio, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb reflections pan
        /// </summary>
        public Vector3 ReflectionsPan
        {
            get
            {
                if (_isEAX == false)
                    return Vector3.Zero;
                var val = new float[3];
                alGetEffectfv(_effect, ALReverbEAX.ReflectionsPan, val);
                alHandleError("failed to get reverb variable");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.ReflectionsPan, new float[]{ value.X, value.Y, value.Z });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb late reverb pan
        /// </summary>
        public Vector3 LateReverbPan
        {
            get
            {
                if (_isEAX == false)
                    return Vector3.Zero;
                var val = new float[3];
                alGetEffectfv(_effect, ALReverbEAX.LateReverbPan, val);
                alHandleError("failed to get reverb variable");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.LateReverbPan, new float[]{ value.X, value.Y, value.Z });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb echo time
        /// </summary>
        public float EchoTime
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.EchoTime, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.EchoTime, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb echo depth
        /// </summary>
        public float EchoDepth
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.EchoDepth, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.EchoDepth, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb modulation time
        /// </summary>
        public float ModulationTime
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.ModulationTime, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.ModulationTime, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb modulation depth
        /// </summary>
        public float ModulationDepth
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.ModulationDepth, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.ModulationDepth, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb hf reference
        /// </summary>
        public float HFReference
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.HFReference, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.HFReference, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// reverb lf reference
        /// </summary>
        public float LFReference
        {
            get
            {
                if (_isEAX == false)
                    return 0;
                var val = new float[1];
                alGetEffectfv(_effect, ALReverbEAX.LFReference, val);
                alHandleError("failed to get reverb variable");
                return val[0];
            }
            set
            {
                if (_isEAX == false)
                    return;
                alEffectfv(_effect, ALReverbEAX.LFReference, new float[]{ value });
                alHandleError("failed to set reverb variable");
            }
        }

        /// <summary>
        /// constructor for reverb
        /// </summary>
        public Reverb() : base()
        {
            this._isEAX = alGetEnumValue("AL_EFFECT_EAXREVERB") != 0;

            if (_isEAX)
                alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.ReverbEAX });
            else
                alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Reverb });
            alHandleError("failed to setup reverb: ");
        }
    }
}