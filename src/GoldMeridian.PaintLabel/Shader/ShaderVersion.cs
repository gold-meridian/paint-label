using System;
using System.Diagnostics;
using System.IO;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.Shader;

public enum ShaderType : ushort
{
    Unknown,
    PixelShader = 0xFFFF,
    VertexShader = 0xFFFE,
    Preshader = 0x4658,
}

public struct ShaderVersion
{
    public ShaderType Type { get; set; }

    public uint Major { get; set; }

    public uint Minor { get; set; }

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
