using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// vocal morpher audio effect
    /// </summary>
    public class VocalMorpher : AudioEffect
    {
        /// <summary>
        /// type of audio effect
        /// </summary>
        public override AudioEffectType Type => AudioEffectType.VocalMorpher;

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
        /// vocal morpher phoneme a
        /// </summary>
        public int PhonemeA
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALVocalMorpher.PhonemeA, val);
                alHandleError("failed to get vocal morpher variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALVocalMorpher.PhonemeA, new int[]{ value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }

        /// <summary>
        /// vocal morpher phoneme b
        /// </summary>
        public int PhonemeB
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALVocalMorpher.PhonemeB, val);
                alHandleError("failed to get vocal morpher variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALVocalMorpher.PhonemeB, new int[]{ value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }

        /// <summary>
        /// vocal morpher phoneme a coarse tuning
        /// </summary>
        public int PhonemeACoarseTuning
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALVocalMorpher.PhonemeACoarseTuning, val);
                alHandleError("failed to get vocal morpher variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALVocalMorpher.PhonemeACoarseTuning, new int[]{ value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }
        
        /// <summary>
        /// vocal morpher phoneme b coarse tuning
        /// </summary>
        public int PhonemeBCoarseTuning
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALVocalMorpher.PhonemeBCoarseTuning, val);
                alHandleError("failed to get vocal morpher variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALVocalMorpher.PhonemeBCoarseTuning, new int[]{ value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }

        /// <summary>
        /// vocal morpher waveform
        /// </summary>
        public WaveformType Waveform
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALVocalMorpher.Waveform, val);
                alHandleError("failed to get vocal morpher variable: ");
                return (WaveformType)val[0];
            }
            set
            {
                alEffectiv(_effect, ALVocalMorpher.Waveform, new int[]{ (int)value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }

        /// <summary>
        /// vocal morpher rate
        /// </summary>
        public float Rate
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALVocalMorpher.Rate, val);
                alHandleError("failed to get vocal morpher variable: ");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALVocalMorpher.Rate, new float[]{ value });
                alHandleError("failed to set vocal morpher variable: ");
            }
        }

        /// <summary>
        /// constructor for vocal morpher
        /// </summary>
        public VocalMorpher() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.VocalMorpher });
            alHandleError("failed to setup vocal morpher: ");
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.Effect, new int[]{ (int)_effect });
            alHandleError("failed to setup effect slot: ");
        }
    }
}