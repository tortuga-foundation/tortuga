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
    }
}