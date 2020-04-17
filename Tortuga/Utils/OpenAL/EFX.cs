namespace Tortuga.Utils.OpenAL
{
    internal unsafe static partial class OpenALNative
    {
        private delegate void alGenEffects_T(int size, out uint effect);
        private static alGenEffects_T _alGenEffects = LoadFunction<alGenEffects_T>("alGenEffects");
        public static void alGenEffects(out uint effect) => _alGenEffects(1, out effect);

        private delegate void alDeleteEffects_T(int size, uint[] effects);
        private static alDeleteEffects_T _alDeleteEffects = LoadFunction<alDeleteEffects_T>("alDeleteEffects");
        public static void alDeleteEffects(uint effect) => _alDeleteEffects(1, new uint[]{ effect });
    
        
    }
}