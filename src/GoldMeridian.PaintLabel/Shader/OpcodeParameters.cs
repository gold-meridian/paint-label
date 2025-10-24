namespace GoldMeridian.PaintLabel.Shader;

public abstract class OpcodeParameter
{
    public uint Register { get; set; }

    public RegisterType RegisterType { get; set; }
}

public sealed class SourceParameter : OpcodeParameter
{
    public bool WriteX { get; set; }

    public bool WriteY { get; set; }

    public bool WriteZ { get; set; }

    public bool WriteW { get; set; }
}

public sealed class DestinationParameter : OpcodeParameter
{
    public bool RelativeAddressing { get; set; }

    public SourceMod Modifier { get; set; }

    public Swizzle SwizzleX { get; set; }

    public Swizzle SwizzleY { get; set; }

    public Swizzle SwizzleZ { get; set; }

    public Swizzle SwizzleW { get; set; }
}
