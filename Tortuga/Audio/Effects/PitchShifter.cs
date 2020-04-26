using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// pitch shifter audio effect
    /// </summary>
    public class PitchShifter : AudioEffect
    {
        /// <summary>
        /// pitch shifter phoneme a
        /// </summary>
        public int CoarseTune
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALPitchShifter.CoarseTune, val);
                alHandleError("failed to get pitch shifter variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALPitchShifter.CoarseTune, new int[]{ value });
                alHandleError("failed to set pitch shifter variable: ");
            }
        }

        /// <summary>
        /// pitch shifter phoneme b
        /// </summary>
        public int FineTune
        {
            get
            {
                var val = new int[1];
                alGetEffectiv(_effect, ALPitchShifter.FineTune, val);
                alHandleError("failed to get pitch shifter variable: ");
                return val[0];
            }
            set
            {
                alEffectiv(_effect, ALPitchShifter.FineTune, new int[]{ value });
                alHandleError("failed to set pitch shifter variable: ");
            }
        }

        /// <summary>
        /// constructor for pitch shifter
        /// </summary>
        public PitchShifter() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.PitchShifter });
            alHandleError("failed to setup pitch shifter: ");
        }
    }
}