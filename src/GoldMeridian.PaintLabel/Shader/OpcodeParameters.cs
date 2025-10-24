using System.IO;

namespace GoldMeridian.PaintLabel.Shader;

public abstract class OpcodeParameter
{
    public uint Register { get; set; }

    public RegisterType RegisterType { get; set; }
}

public sealed class SourceParameter : OpcodeParameter
{
    public bool RelativeAddressing { get; set; }

    public SourceMod Modifier { get; set; }

    public Swizzle SwizzleX { get; set; }

    public Swizzle SwizzleY { get; set; }

    public Swizzle SwizzleZ { get; set; }

    public Swizzle SwizzleW { get; set; }

    public static SourceParameter Read(BinaryReader reader)
    {
        var token = new BitNumber(reader.ReadUInt32());

        var param = new SourceParameter
        {
            Register = token[0, 10],
            RegisterType = (RegisterType)(token[11, 12] << 3 | token[28, 30]),
            RelativeAddressing = token[13],
            SwizzleX = (Swizzle)token[16, 17],
            SwizzleY = (Swizzle)token[18, 19],
            SwizzleZ = (Swizzle)token[20, 21],
            SwizzleW = (Swizzle)token[22, 23],
            Modifier = (SourceMod)token[24, 27],
        };

        // ShaderDecompiler stuff we don't care about.
        /*
        if (param.RegisterType == ParameterRegisterType.Address && version.Type == ShaderType.PixelShader)
            param.RegisterType = ParameterRegisterType.Texture;

        if (param.RegisterType == ParameterRegisterType.Output && version.CheckVersionLess(ShaderType.VertexShader, 3, 0))
            param.RegisterType = ParameterRegisterType.Texcrdout;
        */

        return param;
    }
}

public sealed class DestinationParameter : OpcodeParameter
{
    public bool WriteX { get; set; }

    public bool WriteY { get; set; }

    public bool WriteZ { get; set; }

    public bool WriteW { get; set; }

    public static DestinationParameter Read(BinaryReader reader)
    {
        var token = new BitNumber(reader.ReadUInt32());

        var param = new DestinationParameter
        {
            Register = token[0, 10],
            RegisterType = (RegisterType)(token[11, 12] << 3 | token[28, 30]),
            WriteX = token[16],
            WriteY = token[17],
            WriteZ = token[18],
            WriteW = token[19],
        };

        // ShaderDecompiler stuff we don't care about.
        /*
        if (param.RegisterType == RegisterType.Address && version.Type == ShaderType.PixelShader)
            param.RegisterType = RegisterType.Texture;

        if (param.RegisterType == RegisterType.Output && version.CheckVersionLess(ShaderType.VertexShader, 3, 0))
            param.RegisterType = RegisterType.Texcrdout;
        */

        return param;
    }
}
