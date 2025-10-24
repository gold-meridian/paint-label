using System.IO;
using System.Text;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.Shader;

public sealed class Shader : BaseShader<ShaderOpcode>
{
    private const uint header_ctab = 'C' | 'T' << 8 | 'A' << 16 | 'B' << 24;
    private const uint header_pres = 'P' | 'R' << 8 | 'E' << 16 | 'S' << 24;

    public Preshader? Preshader { get; set; }

    public string? Creator { get; private set; }

    public string? Target { get; private set; }

    public static Shader ReadShader(EffectReader reader)
    {
        var shader = new Shader
        {
            Version = ShaderVersion.Read(reader.Reader),
        };

        while (shader.ReadOpcode(reader.Reader)) { }

        return shader;
    }

    protected override bool ProcessSpecialComments(BinaryReader reader, uint length)
    {
        throw new System.NotImplementedException();
    }
    
    protected override OpcodeData<ShaderOpcode> MakeRegularToken(BinaryReader reader, uint tokenType, uint length)
    {
        throw new System.NotImplementedException();
    }
    
    protected override OpcodeData<ShaderOpcode> MakeComment(uint tokenType, uint length, byte[] readBytes)
    {
        return new OpcodeData<ShaderOpcode>
        {
            Type = (ShaderOpcode)tokenType,
            Length = length,
            Comment = Encoding.ASCII.GetString(readBytes),
        };
    }
}
