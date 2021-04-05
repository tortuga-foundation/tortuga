using System;
using System.Numerics;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Audio
{
    /// <summary>
    /// Audio listener component
    /// </summary>
    public class AudioListener : Core.BaseComponent
    {
        /// <summary>
        /// Current position of audio listener
        /// </summary>
        public Vector3 Position
        {
            get
            {
                var val = new float[3];
                alGetListenerfv(ALListener.Position, val);
                alHandleError("failed to get listener position");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alListenerfv(ALListener.Position, new float[]{ value.X, value.Y, value.Z });
                alHandleError("failed to set listener position");
            }
        }

        /// <summary>
        /// Velocity of audio listener
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                var val = new float[3];
                alGetListenerfv(ALListener.Velocity, val);
                alHandleError("failed to get listener velocity");
                return new Vector3(val[0], val[1], val[2]);
            }
            set
            {
                alListenerfv(ALListener.Velocity, new float[]{ value.X, value.Y, value.Z });
                alHandleError("failed to set listener velocity");
            }
        } 
    
        /// <summary>
        /// Set's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void SetOrientation(Vector3 up, Vector3 forward)
        {
            alListenerfv(ALListener.Orientation, new float[]{
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z 
            });
            alHandleError("failed to set listener orientation: ");
        }

        /// <summary>
        /// Get's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void GetOrientation(out Vector3 up, out Vector3 forward)
        {
            var vals = new float[6];
            alGetListenerfv(ALListener.Orientation, vals);
            forward = new Vector3(vals[0], vals[1], vals[2]);
            up = new Vector3(vals[3], vals[4], vals[5]);
            alHandleError("failed to get listener orientation: ");
        }
    }
}