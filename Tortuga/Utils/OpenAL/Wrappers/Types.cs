namespace Tortuga.Utils.OpenAL
{
    internal enum ALSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Pause = 0x1013,
        Stopped = 0x1014
    }

    internal enum ALSourceType
    {
        Static = 0x1028,
        Streaming = 0x1029,
        Undetermined = 0x1030
    }

    internal enum ALBufferState
    {
        UnUsed = 0x2010,
        Pending = 0x2011,
        Processed = 0x2012
    }

    internal enum ALFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103
    }

    internal enum ALError
    {
        None = 0,
        InvalidName = 0xA001,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005
    }

    internal enum ALContextString
    {
        Vendor = 0xB001,
        Version = 0xB002,
        Renderer = 0xB003,
        Extensions = 0xB004
    }

    internal enum ALSource
    { 
        SourceRelative = 0x202,
        ConeInnerAngle = 0x1001,
        ConeOuterAngle = 0x1002,
        Pitch = 0x1003,
        Position = 0x1004,
        Direction = 0x1005,
        Velocity = 0x1006,
        Looping = 0x1007,
        Buffer = 0x1009,
        Gain = 0x100A,
        MinGain = 0x100D,
        MaxGain = 0x100E,
        Orientation = 0x100F,
        SourceState = 0x1010,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
        ReferenceDistance = 0x1020,
        RolloffFactor = 0x1021,
        ConeOuterGain = 0x1022,
        MaxDistance = 0x1023,
        SecOffset = 0x1024,
        SampleOffset = 0x1025,
        ByteOffset = 0x1026,
        SourceType = 0x1027,
        Frequency = 0x2001,
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,
        DirectFilter = 0x20005,
        AuxiliarySendFilter = 0x20006,
        AirAbsorptionFactor = 0x20007,
        RoomRollOffFactor = 0x20008,
        ConeOuterGainHF = 0x20009,
        DirectFilterGainHFAuto = 0x2000A,
        AuxiliarySendFilterGainAuto = 0x2000B,
        AuxiliarySendFilterGainHFAuto = 0x2000C,
    }

    internal enum ALListener
    {
        Position = 0x1004,
        Velocity = 0x1006,
        Orientation = 0x100F,
        MetersPerUnit = 0x20004,
    }

    internal enum ALDistanceModel
    {
        None = 0xD000,
        InverseDistance = 0xD001,
        InverseDistanceClamped = 0xD002,
        LinearDistance = 0xD003,
        LinearDistanceClamped = 0xD004,
        ExponentDistance = 0xD005,
        ExponentDistanceClamped = 0xD006
    }

    internal enum ALReverb
    {
        Density = 0x0001,
        Diffusion = 0x0002,
        Gain = 0x0003,
        GainHF = 0x0004,
        DecayTime = 0x0005,
        DecayHFRatio = 0x0006,
        ReflectionsGain = 0x0007,
        ReflectionsDelay = 0x0008,
        LateReverbGain = 0x0009,
        LateReverbDelay = 0x000A,
        AirAbsorptionGainHF = 0x000B,
        RoomRollOffFactor = 0x000C,
        DecayHFLimit = 0x000D
    }

    internal enum ALReverbEAX
    {
        Density = 0x0001,
        Diffusion = 0x0002,
        Gain = 0x0003,
        GainHF = 0x0004,
        GainLF = 0x0005,
        DecayTime = 0x0006,
        DecayHFRatio = 0x0007,
        DecayLFRatio = 0x0008,
        ReflectionsGain = 0x0009,
        ReflectionsDelay = 0x000A,
        ReflectionsPan = 0x000B,
        LateReverbGain = 0x000C,
        LateReverbDelay = 0x000D,
        LateReverbPan = 0x000E,
        EchoTime = 0x000F,
        EchoDepth = 0x0010,
        ModulationTime = 0x0011,
        ModulationDepth = 0x0012,
        AirAbsorptionGainHF = 0x0013,
        HFReference = 0x0014,
        LFReference = 0x0015,
        RoomRollOffFactor = 0x0016,
        DecayHFLimit = 0x0017
    }

    internal enum ALChorus
    {
        Waveform = 0x0001,
        Phase = 0x0002,
        Rate = 0x0003,
        Depth = 0x0004,
        Feedback = 0x0005,
        Delay = 0x0006
    }

    internal enum ALDistortion
    {
        Edge = 0x0001,
        Gain = 0x0002,
        LowPassCutOff = 0x0003,
        EQCenter = 0x0004,
        EQBandwidth = 0x0005
    }

    internal enum ALEcho
    {
        Delay = 0x0001,
        LRDelay = 0x0002,
        Damping = 0x0003,
        Feedback = 0x0004,
        Spread = 0x0005
    }

    internal enum ALFlanger
    {
        Waveform = 0x0001,
        Phase = 0x0002,
        Rate = 0x0003,
        Depth = 0x0004,
        Feedback = 0x0005,
        Delay = 0x0006
    }

    internal enum ALFrequencyShifter
    {
        Frequency = 0x0001,
        LeftDirection = 0x0002,
        RightDirection = 0x0003
    }

    internal enum ALVocalMorpher
    {
        PhonemeA = 0x0001,
        PhonemeACoarseTuning = 0x0002,
        PhonemeB = 0x0003,
        PhonemeBCoarseTuning = 0x0004,
        Waveform = 0x0005,
        Rate = 0x0006
    }

    internal enum ALPitchShifter
    {
        CoarseTune = 0x0001,
        FineTune = 0x0002
    }

    internal enum ALRingModulator
    {
        Frequency = 0x0001,
        HighPassCutOff = 0x0002,
        Waveform = 0x0003
    }

    internal enum ALAutoWah
    {
        AttackTime = 0x0001,
        ReleaseTime = 0x0002,
        Resonance = 0x0003,
        PeakGain = 0x0004
    }

    internal enum ALCompressor
    {
        OnOff = 0x0001
    }

    internal enum ALEqualizer
    {
        LowGain = 0x0001,
        LowCutOff = 0x0002,
        Mid1Gain = 0x0003,
        Mid1Center = 0x0004,
        Mid1Width = 0x0005,
        Mid2Gain = 0x0006,
        Mid2Center = 0x0007,
        Mid2Width = 0x0008,
        HighGain = 0x0009,
        HighCutOff = 0x000A
    }

    internal enum ALEffect
    {
        Type = 0x8001,
        None = 0x0000,
        Reverb = 0x0001,
        Chorus = 0x0002,
        Distortion = 0x0003,
        Echo = 0x0004,
        Flanger = 0x0005,
        FrequencyShifter = 0x0006,
        VocalMorpher = 0x0007,
        PitchShifter = 0x0008,
        RingModulator = 0x0009,
        AutoWah = 0x000A,
        Compressor = 0x000B,
        Equalizer = 0x000C,
        ReverbEAX = 0x8000
    }

    internal enum ALAuxiliaryEffectSlot
    {
        Effect = 0x0001,
        Gain = 0x0002,
        AxuiliarySendAuto = 0x0003
    }

    internal enum ALLowPassFilter
    {
        Gain = 0x0001,
        GainHF = 0x0002
    }

    internal enum ALHighPassFilter
    {
        Gain = 0x0001,
        GainLF = 0x0002
    }

    internal enum ALBandPassFilter
    {
        Gain = 0x0001,
        GainLF = 0x0002,
        GainHF = 0x0003
    }

    internal enum ALFilter
    {
        Type = 0x8001,
        None = 0x0000,
        LowPass = 0x0001,
        HighPass = 0x0002,
        BandPass = 0x0003
    }
}