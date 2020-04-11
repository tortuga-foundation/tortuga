using NAudio.Wave;
using System.Threading.Tasks;

namespace Tortuga.Audio
{
    /// <summary>
    /// A loaded audio clip from a file
    /// </summary>
    public class AudioClip
    {
        /// <summary>
        /// NAudio file stream
        /// </summary>
        public WaveStream Stream => _stream;
        private WaveStream _stream;

        /// <summary>
        /// Constructor for audio clip
        /// </summary>
        /// <param name="stream">NAudio wav stream</param>
        public AudioClip(WaveStream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Loads a audio clip into memory
        /// </summary>
        /// <param name="file">file to load</param>
        /// <returns>Audio clip object</returns>
        public static Task<AudioClip> Load(string file)
        {
            WaveStream stream;
            if (file.EndsWith(".wav"))
                stream = new WaveFileReader(file);
            else if (file.EndsWith(".mp3"))
                stream = new Mp3FileReader(file);
            else
                throw new System.NotSupportedException("This file format is not currently supported");
        
            var clip = new AudioClip(stream);
            return Task.FromResult(clip);
        }
    }
}