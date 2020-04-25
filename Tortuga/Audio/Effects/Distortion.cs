using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio.API
{
    /// <summary>
    /// distortion audio effect
    /// </summary>
    public class Distortion : AudioEffect
    {
        /// <summary>
        /// distortion edge
        /// </summary>
        public float Edge
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.Edge, val);
                alHandleError("failed to set distortion variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.Edge, new float[]{ value });
                alHandleError("failed to set distortion variable");
            }
        }

        /// <summary>
        /// distortion gain
        /// </summary>
        public float Gain
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.Gain, val);
                alHandleError("failed to set distortion variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.Gain, new float[]{ value });
                alHandleError("failed to set distortion variable");
            }
        }

        /// <summary>
        /// distortion low pass cut off
        /// </summary>
        public float LowPassCutOff
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.LowPassCutOff, val);
                alHandleError("failed to set distortion variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.LowPassCutOff, new float[]{ value });
                alHandleError("failed to set distortion variable");
            }
        }

        /// <summary>
        /// distortion eq center
        /// </summary>
        public float EQCenter
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.EQCenter, val);
                alHandleError("failed to set distortion variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.EQCenter, new float[]{ value });
                alHandleError("failed to set distortion variable");
            }
        }

        /// <summary>
        /// distortion eq bandwidth
        /// </summary>
        public float EQBandwidth
        {
            get
            {
                var val = new float[1];
                alGetEffectfv(_effect, ALDistortion.EQBandwidth, val);
                alHandleError("failed to set distortion variable");
                return val[0];
            }
            set
            {
                alEffectfv(_effect, ALDistortion.EQBandwidth, new float[]{ value });
                alHandleError("failed to set distortion variable");
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
}