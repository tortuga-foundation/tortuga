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
}