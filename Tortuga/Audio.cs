using System;
using Tortuga.Utils.OpenAL;
using Tortuga.Graphics.API;
using static Tortuga.Utils.OpenAL.OpenALNative;

public class Audio
{
    private ALCdevice _device;
    private ALCcontext _context;

    public Audio()
    {
        _device = alcOpenDevice(null);
        _context = alcCreateContext(_device);
        alcMakeContextCurrent(_context);
    
        alGenBuffers(1, out uint buffer);

        int sampleRate = 22050;
        int bufferSize = 30 * sampleRate;

        var samples = new NativeList<short>((uint)bufferSize);
        samples.Count = (uint)bufferSize;
        for (int i = 0; i < samples.Count; i++)
            samples[i] = (short)(32760 * MathF.Sin((2.0f * MathF.PI * 440.0f) / (sampleRate * i)));

        unsafe
        {
            alBufferData(buffer, ALFormat.Mono16, new IntPtr(samples.Data.ToPointer()), bufferSize, sampleRate);
        } 

        alGenSources(1, out uint source);
        alSourcei(source, ALParams.Buffer, (int)buffer);
        alSourcePlay(source);
    }
    ~Audio()
    {
        alcDestroyContext(_context);
        alcCloseDevice(_device);
    }
}