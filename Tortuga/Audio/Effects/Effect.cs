using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// different types of audio effects
    /// </summary>
    public enum AudioEffectType
    {
        /// <summary>
        /// None
        /// </summary>
        None,
        /// <summary>
        /// AutoWah
        /// </summary>
        AutoWah,
        /// <summary>
        /// Chorus
        /// </summary>
        Chorus,
        /// <summary>
        /// Compressor
        /// </summary>
        Compressor,
        /// <summary>
        /// Distortion
        /// </summary>
        Distortion,
        /// <summary>
        /// Echo
        /// </summary>
        Echo,
        /// <summary>
        /// Equalizer
        /// </summary>
        Equalizer,
        /// <summary>
        /// Flanger
        /// </summary>
        Flanger,
        /// <summary>
        /// FrequencyShifter
        /// </summary>
        FrequencyShifter,
        /// <summary>
        /// PitchShifter
        /// </summary>
        PitchShifter,
        /// <summary>
        /// Reverb
        /// </summary>
        Reverb,
        /// <summary>
        /// RingModulator
        /// </summary>
        RingModulator,
        /// <summary>
        /// VocalMorpher
        /// </summary>
        VocalMorpher
    }

    /// <summary>
    /// base audio effect class
    /// </summary>
    public abstract class AudioEffect
    {
        /// <summary>
        /// type of audio effect
        /// </summary>
        public virtual AudioEffectType Type => AudioEffectType.None;

        /// <summary>
        /// filter on the effect
        /// </summary>
        public Filter Filter
        {
            get => _filter;
            set
            {
                _filter = value;
                IsDirty = true;
            }
        }
        private Filter _filter = null;
        /// <summary>
        /// When a new filter is applied the audio effect is marked 
        /// as dirty so the source can reload the effect with the filter
        /// </summary>
        internal bool IsDirty;

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

            if (alIsEffect(_effect) == false)
                throw new System.Exception("failed to create open al effect");
            IsDirty = false;
        }
        /// <summary>
        /// de-constructor for audio effect
        /// </summary>
        ~AudioEffect()
        {
            alDeleteAuxiliaryEffectSlots(new uint[]{ _aux });
            alHandleError("failed to delete effect auxiliary slot");
            alDeleteEffects(_effect);
            alHandleError("failed to delete effect");
        }
    }
}