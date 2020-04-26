using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// ring modulator audio effect
    /// </summary>
    public class RingModulator : AudioEffect
    {
        /// <summary>
        /// type of audio effect
        /// </summary>
        public override AudioEffectType Type => AudioEffectType.RingModulator;

        /// <summary>
        /// Types of waveform
        /// </summary>
        public enum WaveformType
        {
            /// <summary>
            /// Sin
            /// </summary>
            Sin  = 0,
            /// <summary>
            /// Tirangle
            /// </summary>
            Triangle = 1,
            /// <summary>
            /// Square
            /// </summary> 
            Square = 2,
        }

        /// <summary>
        /// ring modulator frequency
        /// </summary>
        public float Frequency
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALRingModulator.Frequency, val);
                alHandleError("failed to get ring modulator variable: ");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALRingModulator.Frequency, new float[]{ value });
                alHandleError("failed to set ring modulator variable: ");
            }
        }

        /// <summary>
        /// ring modulator high pass cut off
        /// </summary>
        public float HighPassCutOff
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALRingModulator.HighPassCutOff, val);
                alHandleError("failed to get ring modulator variable: ");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALRingModulator.HighPassCutOff, new float[]{ value });
                alHandleError("failed to set ring modulator variable: ");
            }
        }

        /// <summary>
        /// ring modulator waveform
        /// </summary>
        public WaveformType Waveform
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALRingModulator.Waveform, val);
                alHandleError("failed to get ring modulator variable: ");
                return (WaveformType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALRingModulator.Waveform, new int[]{ (int)value });
                alHandleError("failed to set ring modulator variable: ");
            }
        }

        /// <summary>
        /// constructor for ring modulator
        /// </summary>
        public RingModulator() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.PitchShifter });
            alHandleError("failed to setup ring modulator: ");
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.Effect, new int[]{ (int)_effect });
            alHandleError("failed to setup effect slot: ");
        }
    }
}