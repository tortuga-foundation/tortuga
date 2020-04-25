using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.Effect
{
    /// <summary>
    /// echo audio effect
    /// </summary>
    public class Echo : AudioEffect
    {

        /// <summary>
        /// echo delay
        /// </summary>
        public float Delay
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEcho.Delay, val);
                alHandleError("failed to get echo variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEcho.Delay, new float[]{ value });
                alHandleError("failed to set echo variable");
            }
        }

        /// <summary>
        /// echo LR delay
        /// </summary>
        public float LRDelay
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEcho.LRDelay, val);
                alHandleError("failed to get echo variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEcho.LRDelay, new float[]{ value });
                alHandleError("failed to set echo variable");
            }
        }

        /// <summary>
        /// echo feedback
        /// </summary>
        public float Feedback
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEcho.Feedback, val);
                alHandleError("failed to get echo variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEcho.Feedback, new float[]{ value });
                alHandleError("failed to set echo variable");
            }
        }

        /// <summary>
        /// echo spread
        /// </summary>
        public float Spread
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEcho.Spread, val);
                alHandleError("failed to get echo variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEcho.Spread, new float[]{ value });
                alHandleError("failed to set echo variable");
            }
        }

        /// <summary>
        /// echo damping
        /// </summary>
        public float Damping
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALEcho.Damping, val);
                alHandleError("failed to get echo variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALEcho.Damping, new float[]{ value });
                alHandleError("failed to set echo variable");
            }
        }

        /// <summary>
        /// constructor for echo
        /// </summary>
        public Echo() : base()
        {
            alEffectiv(_effect, ALEffect.Type, new int[]{ (int)ALEffect.Echo });
            alHandleError("failed to setup echo: ");
        }
    }
}