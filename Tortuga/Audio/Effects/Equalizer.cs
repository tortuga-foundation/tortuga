using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// dequalizer audio effect
    /// </summary>
    public class Equalizer : AudioEffect
    {
        /// <summary>
        /// type of audio effect
        /// </summary>
        public override AudioEffectType Type => AudioEffectType.Equalizer;

        /// <summary>
        /// equalizer low gain
        /// </summary>
        public float LowGain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.LowGain, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.LowGain, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer low cut off
        /// </summary>
        public float LowCutOff
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.LowCutOff, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.LowCutOff, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 1 gain
        /// </summary>
        public float Mid1Gain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid1Gain, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid1Gain, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 1 center
        /// </summary>
        public float Mid1Center
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid1Center, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid1Center, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 1 width
        /// </summary>
        public float Mid1Width
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid1Width, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid1Width, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 2 gain
        /// </summary>
        public float Mid2Gain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid2Gain, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid2Gain, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 2 center
        /// </summary>
        public float Mid2Center
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid2Center, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid2Center, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer mid 2 width
        /// </summary>
        public float Mid2Width
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.Mid2Width, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.Mid2Width, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer high gain
        /// </summary>
        public float HighGain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.HighGain, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.HighGain, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// equalizer high cut off
        /// </summary>
        public float HighCutOff
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEqualizer.HighCutOff, val);
                alHandleError("failed to get dequalizer variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEqualizer.HighCutOff, new float[]{ value });
                alHandleError("failed to set dequalizer variable");
            }
        }

        /// <summary>
        /// constructor for dequalizer
        /// </summary>
        public Equalizer() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Equalizer });
            alHandleError("failed to setup dequalizer: ");
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.Effect, new int[]{ (int)_effect });
            alHandleError("failed to setup effect slot: ");
        }
    }
}