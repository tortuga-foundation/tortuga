using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.API
{
    /// <summary>
    /// base audio effect class
    /// </summary>
    public abstract class AudioEffect
    {
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
            alGenEffects(out uint effect);
            alHandleError("failed to generate effect");
            _effect = effect;
            alGenAuxiliaryEffectSlots(out uint aux);
            alHandleError("failed to generate effect auxiliary slot");
            _aux = aux;
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
            alAuxiliaryEffectSlotiv(_aux, ALAuxiliaryEffectSlot.Effect, new int[]{ (int)_effect });
        }
    }

    /// <summary>
    /// Distortion effect
    /// </summary>
    public class Distortion : AudioEffect
    {
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