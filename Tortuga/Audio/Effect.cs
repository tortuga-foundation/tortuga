using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.API
{
    /// <summary>
    /// base audio effect class
    /// </summary>
    public abstract class AudioEffect
    {
        internal uint AuxiliarySlot => _aux;
        internal uint Handle => _effect;
        /// <summary>
        /// open al effect handler
        /// </summary>
        protected uint _effect;
        /// <summary>
        /// open al auxiliary effect slot
        /// </summary>
        protected uint _aux;

        /// <summary>
        /// constructor for audio effect
        /// </summary>
        public AudioEffect()
        {
            alGenEffects(out _effect);
            alHandleError("failed to generate effect");
            alGenAuxiliaryEffectSlots(out _aux);
            alHandleError("failed to generate effect auxiliary slot");
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.Effect, new int[]{ (int)_effect });
            alHandleError("failed to setup effect slot: ");
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.AxuiliarySendAuto, new int[]{ 1 });
            alHandleError("failed to setup effect slot: ");

            if (alIsEffect(_effect) == false)
                throw new System.Exception("failed to create open al effect");
        }
        /// <summary>
        /// de-constructor for audio effect
        /// </summary>
        ~AudioEffect()
        {
            alDeleteAuxiliaryEffectSlots(new uint[]{ _aux });
            alDeleteEffects(_effect);
            alHandleError("failed to delete effect");
        }
    }

    /// <summary>
    /// Reverb effect
    /// </summary>
    public class Reverb : AudioEffect
    {
        /// <summary>
        /// constructor for reverb
        /// </summary>
        public Reverb() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Reverb });
            alHandleError("failed to setup reverb: ");
        }
    }

    /// <summary>
    /// Distortion effect
    /// </summary>
    public class Distortion : AudioEffect
    {
        public float Edge
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.Edge, val);
                alHandleError("failed to get distortion effect edge: ");
                return val[0];
            }
            set
            {
                alGetEffectfv(_effect, ALDistortion.Edge, new float[]{ value });
                alHandleError("failed to set distortion effect edge: ");
            }
        }

        public float Gain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.Gain, val);
                alHandleError("failed to get distortion effect gain: ");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.Gain, new float[]{ value });
                alHandleError("failed to set distortion effect gain: ");
            }
        }

        /// <summary>
        /// constructor for distortion
        /// </summary>
        public Distortion() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Distortion });
            alHandleError("failed to setup distortion: ");
        }
    }

    /// <summary>
    /// Equalizer effect
    /// </summary>
    public class Equalizer : AudioEffect
    {
        /// <summary>
        /// constructor for equalizer
        /// </summary>
        public Equalizer() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Equalizer });
            alHandleError("failed to setup equalizer: ");
        }
    }

}