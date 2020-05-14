using System;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Tortuga.Utils;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        public unsafe byte[] Samples
        {
            get
            {
                var rtn = new byte[NativeSamples.Count];
                Marshal.Copy(NativeSamples.Data, rtn, 0, rtn.Length);
                return rtn;
            }
        }
        internal NativeList<byte> NativeSamples;

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

        private struct WaveChunk
        {
            public string Id;
            public int Size;
            public byte[] bytes;
        }
        private static List<WaveChunk> GetWaveChuncks(byte[] bytes)
        {
            var data = new List<WaveChunk>();
            int cursor = 0;
            while (cursor < bytes.Length)
            {
                var id = Encoding.ASCII.GetString(CopyBytes(bytes, cursor + 0, 4));
                var size = BitConverter.ToInt32(CopyBytes(bytes, cursor + 4, 4));
                var chunkData = CopyBytes(bytes, cursor + 8, size);
                data.Add(new WaveChunk
                {
                    Id = id,
                    Size = size,
                    bytes = chunkData
                });
                cursor += 8 + size;
            }
            return data;
        }

        private static AudioClip WaveLoader(string file)
        {
            try 
            {
                var bytes = File.ReadAllBytes(file);
                var chuncks = GetWaveChuncks(bytes);

                var riff = chuncks.Find((WaveChunk c) => c.Id == "RIFF");
                var format = Encoding.ASCII.GetString(CopyBytes(riff.bytes, 0, 4));
                if (format != "WAVE")
                    throw new FormatException("wave file is not correctly formatted");

                var subChuncks = GetWaveChuncks(CopyBytes(riff.bytes, 4, riff.bytes.Length - 4));
                var fmt = subChuncks.Find((WaveChunk w) => w.Id == "fmt ");
                var audioFormat = BitConverter.ToInt16(CopyBytes(fmt.bytes, 0, 2));
                var numChannels = BitConverter.ToInt16(CopyBytes(fmt.bytes, 2, 2));
                var sampleRate = BitConverter.ToInt32(CopyBytes(fmt.bytes, 4, 4));
                var byteRate = BitConverter.ToInt32(CopyBytes(fmt.bytes, 8, 4));
                var blockAlign = BitConverter.ToInt16(CopyBytes(fmt.bytes, 12, 2));
                var bitsPerSample = BitConverter.ToInt16(CopyBytes(fmt.bytes, 14, 2));
                if (bitsPerSample != 8 && bitsPerSample != 16)
                    throw new NotSupportedException("bits per sample must be 8 or 16");

                var dataChunk = subChuncks.Find((WaveChunk w) => w.Id == "data");
                var rawData = dataChunk.bytes;

                var data = new AudioClip(numChannels, sampleRate, bitsPerSample);
                data.NativeSamples = new NativeList<byte>();
                data.NativeSamples.Count = (uint)rawData.Length;
                for (int i = 0; i < rawData.Length; i++)
                    data.NativeSamples[i] = rawData[i];
                
                return data;
            }
            catch (Exception)
            {
                throw new FormatException("wave file is not correctly formatted");
            }
        }

        /// <summary>
        /// Load audio clip from a file
        /// </summary>
        /// <param name="file">file path</param>
        /// <returns>returns a audio clip object</returns>
        public static Task<AudioClip> Load(string file)
        {
            if (File.Exists(file) == false)
                throw new FileNotFoundException("cannot locate file");

            if (file.ToLower().EndsWith(".wav"))
                return Task.FromResult(WaveLoader(file));
            else
                throw new NotSupportedException("this type of audio format is not supported");
        }
    }
}