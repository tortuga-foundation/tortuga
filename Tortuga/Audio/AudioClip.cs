using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Tortuga.Utils;

namespace Tortuga.Audio
{
    /// <summary>
    /// Audio data loaded from a file or created procedurally
    /// </summary>
    public class AudioClip
    {
        /// <summary>
        /// Number of channels
        /// </summary>
        public int NumberOfChannels => _numberOfChannels;
        private int _numberOfChannels;
        /// <summary>
        /// Controls how many samples should be played per second
        /// </summary>
        public int SampleRate => _sampleRate;
        private int _sampleRate;
        /// <summary>
        /// bits per sample
        /// </summary>
        public int BitsPerSample => _bitsPerSample;
        private int _bitsPerSample;
        /// <summary>
        /// Samples / audio data
        /// </summary>
        public NativeList<byte> Samples;

        /// <summary>
        /// Constructor for audio clip
        /// </summary>
        /// <param name="numberOfChannels">Amount of channels</param>
        /// <param name="sampleRate">sample rate for the audio clip</param>
        /// <param name="bitsPerSample">The amount of bits in one sample, usually 8 or 16</param>
        public AudioClip(int numberOfChannels, int sampleRate, int bitsPerSample)
        {
            _numberOfChannels = numberOfChannels;
            _sampleRate = sampleRate;
            _bitsPerSample = bitsPerSample;
        }

        private static byte[] CopyBytes(byte[] full, int offset, int size)
        {
            var data = new byte[size];
            Array.Copy(full, offset, data, 0, size);
            return data;
        }

        private static AudioClip WaveLoader(string file)
        {
            var bytes = File.ReadAllBytes(file);
            if (bytes.Length < 44)
                throw new FormatException("Wave file is not correctly formatted");
            var chunkId = Encoding.ASCII.GetString(CopyBytes(bytes, 0, 4));
            if (chunkId != "RIFF")
                throw new FormatException("Wave file is not correctly formatted");
            var chunkSize = BitConverter.ToInt32(CopyBytes(bytes, 4, 4));
            var format = Encoding.ASCII.GetString(CopyBytes(bytes, 8, 4));
            if (format != "WAVE")
                throw new FormatException("Wave file is not correctly formatted");
            var subChunk1Id = Encoding.ASCII.GetString(CopyBytes(bytes, 12, 4));
            if (subChunk1Id != "fmt ")
                throw new FormatException("Wave file is not correctly formatted");
            var subChunk1Size = BitConverter.ToInt32(CopyBytes(bytes, 16, 4));
            if (subChunk1Size != 16)
                throw new FormatException("Wave file is not correctly formatted");
            var audioFormat = BitConverter.ToInt16(CopyBytes(bytes, 20, 2));
            if (audioFormat != 1)
                throw new NotSupportedException("compressed wav files are not supported");
            var numChannels = BitConverter.ToInt16(CopyBytes(bytes, 22, 2));
            var sampleRate = BitConverter.ToInt32(CopyBytes(bytes, 24, 4));
            var byteRate = BitConverter.ToInt32(CopyBytes(bytes, 28, 4));
            var blockAlign = BitConverter.ToInt16(CopyBytes(bytes, 32, 2));
            var bitsPerSample = BitConverter.ToInt16(CopyBytes(bytes, 34, 2));
            if (bitsPerSample != 8 && bitsPerSample != 16)
                throw new FormatException("Bit's per sample must be 8 or 16");
            
            var subChunk2Id = Encoding.ASCII.GetString(CopyBytes(bytes, 36, 4));
            if (subChunk2Id != "data")
                throw new FormatException("Wave file is not correctly formatted");
            var subChunk2Size = BitConverter.ToInt32(CopyBytes(bytes, 40, 4));

            var rawData = CopyBytes(bytes, 44, bytes.Length - 44);
            var data = new AudioClip(numChannels, sampleRate, bitsPerSample);
            data.Samples = new NativeList<byte>();
            data.Samples.Count = (uint)rawData.Length;
            for (int i = 0; i < rawData.Length; i++)
                data.Samples[i] = rawData[i];

            return data;
        }

        /// <summary>
        /// Load audio clip from a file
        /// </summary>
        /// <param name="file">file path</param>
        /// <returns>returns a audio clip object</returns>
        public static Task<AudioClip> Load(string file)
        {
            if (file.EndsWith(".wav"))
                return Task.FromResult(WaveLoader(file));
            else
                throw new NotSupportedException("this type of audio format is not supported");
        }
    }
}