using System.Numerics;
using Tortuga.Utils.OpenAL;
using static Tortuga.Utils.OpenAL.OpenALNative;

namespace Tortuga.Components
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
                Vector3 val;
                alGetListener3f(ALParams.Position, out val.X, out val.Y, out val.Z);
                return val;
            }
            set => alListener3f(ALParams.Position, value);
        }

        /// <summary>
        /// Velocity of audio listener
        /// </summary>
        public Vector3 Velocity
        {
            get
            {
                Vector3 val;
                alGetListener3f(ALParams.Velocity, out val.X, out val.Y, out val.Z);
                return val;
            }
            set => alListener3f(ALParams.Velocity, value);
        } 
    
        /// <summary>
        /// Set's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void SetOrientation(Vector3 up, Vector3 forward)
        {
            alListenerfv(ALParams.Orientation, new float[]{
                forward.X, forward.Y, forward.Z,
                up.X, up.Y, up.Z 
            });
        }

        /// <summary>
        /// Get's the audio source orientation
        /// </summary>
        /// <param name="up">up vector</param>
        /// <param name="forward">forward vector</param>
        public void GetOrientation(out Vector3 up, out Vector3 forward)
        {
            alGetListenerfv(ALParams.Orientation, out float[] vals);
            forward = new Vector3(vals[0], vals[1], vals[2]);
            up = new Vector3(vals[3], vals[4], vals[5]);
        }
    }
}