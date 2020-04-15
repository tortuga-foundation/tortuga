using Tortuga.Utils.OpenAL;
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
    
        alGenBuffers(1, out uint[] buffers);
        alGenSources(1, out uint[] sources);
        //alSourcei(source[0], AL_BUFFER, );
    }
    ~Audio()
    {
        alcMakeContextCurrent(null);
        alcDestroyContext(_context);
        alcCloseDevice(_device);
    }
}