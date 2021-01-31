namespace Tortuga.Audio
{
    /// <summary>
    /// Different type of distance models to use for 3D audio
    /// </summary>
    public enum AudioDistanceModel
    {
        /// <summary>
        /// Don't use any distance model
        /// </summary>
        None,
        /// <summary>
        /// Use inverse distance model for audio
        /// </summary>
        InverseDistance,
        /// <summary>
        /// Use linear distance model for audio
        /// </summary>
        LinearDistance,
        /// <summary>
        /// Use exponent distance model for audio
        /// </summary>
        ExponentDistance
    }

    /// <summary>
    /// Audio Settings
    /// </summary>
    public static partial class Settings
    {
        /// <summary>
        /// Type of distance model to use for audio
        /// </summary>
        public static AudioDistanceModel DistanceModel
        {
            get => _distanceModel;
            set
            {
                _distanceModel = value;
                Tortuga.Audio.API.Handler.Device.UpdateDistanceModel();
            }
        }
        private static AudioDistanceModel _distanceModel = AudioDistanceModel.InverseDistance;
        /// <summary>
        /// Clamps the distance model
        /// </summary>
        public static bool IsDistanceModelClamped
        {
            get => _isDistanceModelClamped;
            set
            {
                _isDistanceModelClamped = value;
                Tortuga.Audio.API.Handler.Device.UpdateDistanceModel();
            }
        }
        private static bool _isDistanceModelClamped = true;

        /// <summary>
        /// The speed of sound to use 
        /// </summary>
        public static float SpeedOfSound
        {
            get => _speedOfSound;
            set
            {
                _speedOfSound = value;
                Tortuga.Audio.API.Handler.Device.UpdateSpeedOfSound();
            }
        }
        private static float _speedOfSound = 343.3f;

        /// <summary>
        /// dopler effect
        /// </summary>
        public static float DoplerFactor
        {
            get => _doplerFactor;
            set
            {
                _doplerFactor = value;
                Tortuga.Audio.API.Handler.Device.UpdateDoplerFactor();
            }
        }
        private static float _doplerFactor = 1.0f;
    }
}