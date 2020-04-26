using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// compressor audio effect
    /// </summary>
    public class Compressor : AudioEffect
    {

        /// <summary>
        /// Compressor On
        /// </summary>
        public bool IsOn
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALCompressor.OnOff, val);
                alHandleError("failed to get compressor variable");
                return val[0] == 1;
            }
            set
            {
                var val = 0;
                if (value)
                    val = 1;

                alEffectiv(_effect, ALCompressor.OnOff, new int[]{ val });
                alHandleError("failed to set compressor variable");
            }
        }

        /// <summary>
        /// constructor for compressor
        /// </summary>
        public Compressor() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Compressor });
            alHandleError("failed to setup compressor: ");
        }
    }
}