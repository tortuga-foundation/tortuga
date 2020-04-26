using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// auto wah audio effect
    /// </summary>
    public class AutoWah : AudioEffect
    {

        /// <summary>
        /// auto wah attack time
        /// </summary>
        public float AttackTime
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALAutoWah.AttackTime, val);
                alHandleError("failed to get auto wah variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALAutoWah.AttackTime, new float[]{ value });
                alHandleError("failed to set auto wah variable");
            }
        }

        /// <summary>
        /// auto wah release time
        /// </summary>
        public float ReleaseTime
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALAutoWah.ReleaseTime, val);
                alHandleError("failed to get auto wah variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALAutoWah.ReleaseTime, new float[]{ value });
                alHandleError("failed to set auto wah variable");
            }
        }

        /// <summary>
        /// auto wah resonance
        /// </summary>
        public float Resonance
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALAutoWah.Resonance, val);
                alHandleError("failed to get auto wah variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALAutoWah.Resonance, new float[]{ value });
                alHandleError("failed to set auto wah variable");
            }
        }


        /// <summary>
        /// auto wah peak gain
        /// </summary>
        public float PeakGain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALAutoWah.PeakGain, val);
                alHandleError("failed to get auto wah variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALAutoWah.PeakGain, new float[]{ value });
                alHandleError("failed to set auto wah variable");
            }
        }

        /// <summary>
        /// constructor for auto wah
        /// </summary>
        public AutoWah() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.AutoWah });
            alHandleError("failed to setup auto wah: ");
        }
    }
}