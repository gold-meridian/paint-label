using System;
using System.Diagnostics;
using System.IO;

namespace GoldMeridian.PaintLabel;

public enum ShaderType : ushort
{
    Unknown,
    PixelShader = 0xFFFF,
    VertexShader = 0xFFFE,
    Preshader = 0x4658,
}

public readonly record struct ShaderVersion(
    ShaderType Type,
    uint Major,
    uint Minor
)
{
    public static ShaderVersion Read(BinaryReader reader)
    {
        var token = reader.ReadUInt32();

        var type = (ShaderType)((token & 0xFFFFFF) >> 16);
        if (!Enum.IsDefined(typeof(ShaderType), type))
        {
            Debug.Fail($"Unknown shader type: {type}");
            type = ShaderType.Unknown;
        }

        var minor = token & 0xFF;
        var major = (token & 0xFF00) >> 8;

        var version = new ShaderVersion
        {
            Type = type,
            Major = major,
            Minor = minor,
        };

        return version;
    }
}
