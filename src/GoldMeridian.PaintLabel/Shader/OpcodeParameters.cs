using System.IO;

namespace GoldMeridian.PaintLabel;

public interface IOpcodeParameter
{
    uint Register { get; set; }

    RegisterType RegisterType { get; set; }
}

public struct SourceParameter(BitNumber packed) : IOpcodeParameter
{
    public uint Register
    {
        get => token[0, 10];
        set => token[0, 10] = value;
    }

    public RegisterType RegisterType
    {
        get => (RegisterType)(token[11, 12] << 3 | token[28, 30]);

        set
        {
            var v = (uint)value;
            token[11, 12] = v & 0b11u;
            token[28, 30] = (v >> 3) & 0b111u;
        }
    }

    public bool RelativeAddressing
    {
        get => token[13];
        set => token[13] = value;
    }

    public Swizzle SwizzleX
    {
        get => (Swizzle)token[16, 17];
        set => token[16, 17] = (uint)value;
    }

    public Swizzle SwizzleY
    {
        get => (Swizzle)token[18, 19];
        set => token[18, 19] = (uint)value;
    }

    public Swizzle SwizzleZ
    {
        get => (Swizzle)token[20, 21];
        set => token[20, 21] = (uint)value;
    }

    public Swizzle SwizzleW
    {
        get => (Swizzle)token[22, 23];
        set => token[22, 23] = (uint)value;
    }

    public SourceMod Modifier
    {
        get => (SourceMod)token[24, 27];
        set => token[24, 27] = (uint)value;
    }

    private BitNumber token = packed;

    public static SourceParameter Read(BinaryReader reader)
    {
        return new SourceParameter(new BitNumber(reader.ReadUInt32()));
    }
}

public struct DestinationParameter(BitNumber packed) : IOpcodeParameter
{
    public uint Register
    {
        get => token[0, 10];
        set => token[0, 10] = value;
    }

    public RegisterType RegisterType
    {
        get => (RegisterType)(token[11, 12] << 3 | token[28, 30]);

        set
        {
            var v = (uint)value;
            token[11, 12] = v & 0b11u;
            token[28, 30] = (v >> 3) & 0b111u;
        }
    }

    public bool WriteX
    {
        get => token[16];
        set => token[16] = value;
    }

    public bool WriteY
    {
        get => token[17];
        set => token[17] = value;
    }

    public bool WriteZ
    {
        get => token[18];
        set => token[18] = value;
    }

    public bool WriteW
    {
        get => token[19];
        set => token[19] = value;
    }

    private BitNumber token = packed;

    public static DestinationParameter Read(BinaryReader reader)
    {
        return new DestinationParameter(new BitNumber(reader.ReadUInt32()));
    }
}
