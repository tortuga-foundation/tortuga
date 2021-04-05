namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
        #region effects

        private delegate void alGenEffects_T(int size, out uint effect);
        private static alGenEffects_T _alGenEffects = LoadFunction<alGenEffects_T>("alGenEffects");
        public static void alGenEffects(out uint effect) => _alGenEffects(1, out effect);

        private delegate void alDeleteEffects_T(int size, uint[] effects);
        private static alDeleteEffects_T _alDeleteEffects = LoadFunction<alDeleteEffects_T>("alDeleteEffects");
        public static void alDeleteEffects(uint effect) => _alDeleteEffects(1, new uint[]{ effect });

        private delegate bool alIsEffect_T(uint effect);
        private static alIsEffect_T _alIsEffect = LoadFunction<alIsEffect_T>("alIsEffect");
        public static bool alIsEffect(uint effect) => _alIsEffect(effect);
    
        #endregion

        #region effect ints

        private delegate void alEffectiv_T(uint effect, int param, int[] val);
        private static alEffectiv_T _alEffectiv = LoadFunction<alEffectiv_T>("alEffectiv");
        public static void alEffectiv(uint effect, ALEffect param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALReverbEAX param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALReverb param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALChorus param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALDistortion param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALEcho param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALFlanger param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALFrequencyShifter param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALVocalMorpher param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALPitchShifter param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALRingModulator param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALAutoWah param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALCompressor param, int[] val) => _alEffectiv(effect, (int)param, val);
        public static void alEffectiv(uint effect, ALEqualizer param, int[] val) => _alEffectiv(effect, (int)param, val);
    
        private delegate void alGetEffectiv_T(uint effect, int param, int[] val);
        private static alGetEffectiv_T _alGetEffectiv = LoadFunction<alGetEffectiv_T>("alGetEffectiv");
        public static void alGetEffectiv(uint effect, ALEffect param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALReverbEAX param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALReverb param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALChorus param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALDistortion param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALEcho param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALFlanger param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALFrequencyShifter param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALVocalMorpher param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALPitchShifter param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALRingModulator param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALAutoWah param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALCompressor param, int[] val) => _alGetEffectiv(effect, (int)param, val);
        public static void alGetEffectiv(uint effect, ALEqualizer param, int[] val) => _alGetEffectiv(effect, (int)param, val);

        #endregion

        #region effect float

        private delegate void alEffectfv_T(uint effect, int param, float[] val);
        private static alEffectfv_T _alEffectfv = LoadFunction<alEffectfv_T>("alEffectfv");
        public static void alEffectfv(uint effect, ALEffect param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALReverbEAX param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALReverb param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALChorus param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALDistortion param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALEcho param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALFlanger param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALFrequencyShifter param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALVocalMorpher param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALPitchShifter param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALRingModulator param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALAutoWah param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALCompressor param, float[] val) => _alEffectfv(effect, (int)param, val);
        public static void alEffectfv(uint effect, ALEqualizer param, float[] val) => _alEffectfv(effect, (int)param, val);
    
        private delegate void alGetEffectfv_T(uint effect, int param, float[] val);
        private static alGetEffectfv_T _alGetEffectfv = LoadFunction<alGetEffectfv_T>("alGetEffectfv");
        public static void alGetEffectfv(uint effect, ALEffect param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALReverbEAX param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALReverb param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALChorus param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALDistortion param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALEcho param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALFlanger param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALFrequencyShifter param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALVocalMorpher param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALPitchShifter param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALRingModulator param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALAutoWah param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALCompressor param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        public static void alGetEffectfv(uint effect, ALEqualizer param, float[] val) => _alGetEffectfv(effect, (int)param, val);
        #endregion

        #region effect slot

        private delegate void alGenAuxiliaryEffectSlots_T(int size, out uint aux);
        private static alGenAuxiliaryEffectSlots_T _alGenAuxiliaryEffectSlots = LoadFunction<alGenAuxiliaryEffectSlots_T>("alGenAuxiliaryEffectSlots");
        public static void alGenAuxiliaryEffectSlots(out uint aux) => _alGenAuxiliaryEffectSlots(1, out aux);
    
        private delegate void alDeleteAuxiliaryEffectSlots_T(int size, uint[] effects);
        private static alDeleteAuxiliaryEffectSlots_T _alDeleteAuxiliaryEffectSlots = LoadFunction<alDeleteAuxiliaryEffectSlots_T>("alDeleteAuxiliaryEffectSlots");
        public static void alDeleteAuxiliaryEffectSlots(uint[] effects) => _alDeleteAuxiliaryEffectSlots(effects.Length, effects);

        #endregion

        #region effect slot int

        private delegate void alAuxiliaryEffectSlotiv_T(uint aux, ALAuxiliaryEffectSlot param, int[] val);
        private static alAuxiliaryEffectSlotiv_T _alAuxiliaryEffectSlotiv = LoadFunction<alAuxiliaryEffectSlotiv_T>("alAuxiliaryEffectSlotiv");
        public static void alAuxiliaryEffectSlotiv(uint aux, ALAuxiliaryEffectSlot param, int[] val) => _alAuxiliaryEffectSlotiv(aux, param, val);

        private delegate void alGetAuxiliaryEffectSlotiv_T(uint aux, ALAuxiliaryEffectSlot param, out int[] val);
        private static alGetAuxiliaryEffectSlotiv_T _alGetAuxiliaryEffectSlotiv = LoadFunction<alGetAuxiliaryEffectSlotiv_T>("alGetAuxiliaryEffectSlotiv");
        public static void alGetAuxiliaryEffectSlotiv(uint aux, ALAuxiliaryEffectSlot param, out int[] val) => _alGetAuxiliaryEffectSlotiv(aux, param, out val);

        #endregion
    
        #region filter

        private delegate void alGenFilters_T(int size, out uint filter);
        private static alGenFilters_T _alGenFilters = LoadFunction<alGenFilters_T>("alGenFilters");
        public static void alGenFilters(out uint filter) => _alGenFilters(1, out filter);

        private delegate void alDeleteFilters_T(int size, uint[] filters);
        private static alDeleteFilters_T _alDeleteFilters = LoadFunction<alDeleteFilters_T>("alDeleteFilters");
        public static void alDeleteFilters(uint[] filters) => _alDeleteFilters(filters.Length, filters);
    
        #endregion

        #region filter floats

        private delegate void alFilterfv_T(uint filter, int param, float[] val);
        private static alFilterfv_T _alFilterfv = LoadFunction<alFilterfv_T>("alFilterfv");
        public static void alFilterfv(uint filter, ALFilter param, float[] val) => _alFilterfv(filter, (int)param, val);
        public static void alFilterfv(uint filter, ALLowPassFilter param, float[] val) => _alFilterfv(filter, (int)param, val);
        public static void alFilterfv(uint filter, ALHighPassFilter param, float[] val) => _alFilterfv(filter, (int)param, val);
        public static void alFilterfv(uint filter, ALBandPassFilter param, float[] val) => _alFilterfv(filter, (int)param, val);


        private delegate void alGetFilterfv_T(uint filter, int param, float[] val);
        private static alGetFilterfv_T _alGetFilterfv = LoadFunction<alGetFilterfv_T>("alGetFilterfv");
        public static void alGetFilterfv(uint filter, ALFilter param, float[] val) => _alGetFilterfv(filter, (int)param, val);
        public static void alGetFilterfv(uint filter, ALLowPassFilter param, float[] val) => _alGetFilterfv(filter, (int)param, val);
        public static void alGetFilterfv(uint filter, ALHighPassFilter param, float[] val) => _alGetFilterfv(filter, (int)param, val);
        public static void alGetFilterfv(uint filter, ALBandPassFilter param, float[] val) => _alGetFilterfv(filter, (int)param, val);

        #endregion
    
        #region filter ints

        private delegate void alFilteriv_T(uint filter, int param, int[] val);
        private static alFilteriv_T _alFilteriv = LoadFunction<alFilteriv_T>("alFilteriv");
        public static void alFilteriv(uint filter, ALFilter param, int[] val) => _alFilteriv(filter, (int)param, val);
        public static void alFilteriv(uint filter, ALLowPassFilter param, int[] val) => _alFilteriv(filter, (int)param, val);
        public static void alFilteriv(uint filter, ALHighPassFilter param, int[] val) => _alFilteriv(filter, (int)param, val);
        public static void alFilteriv(uint filter, ALBandPassFilter param, int[] val) => _alFilteriv(filter, (int)param, val);


        private delegate void alGetFilteriv_T(uint filter, int param, int[] val);
        private static alGetFilteriv_T _alGetFilteriv = LoadFunction<alGetFilteriv_T>("alGetFilteriv");
        public static void alGetFilteriv(uint filter, ALFilter param, int[] val) => _alGetFilteriv(filter, (int)param, val);
        public static void alGetFilteriv(uint filter, ALLowPassFilter param, int[] val) => _alGetFilteriv(filter, (int)param, val);
        public static void alGetFilteriv(uint filter, ALHighPassFilter param, int[] val) => _alGetFilteriv(filter, (int)param, val);
        public static void alGetFilteriv(uint filter, ALBandPassFilter param, int[] val) => _alGetFilteriv(filter, (int)param, val);

        #endregion
    }
}