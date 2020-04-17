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

    internal enum ALParams
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
        Size = 0x2004
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
}