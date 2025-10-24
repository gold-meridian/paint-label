using System.IO;
using System.Text;
using GoldMeridian.PaintLabel.IO;

namespace GoldMeridian.PaintLabel.Shader;

public sealed class Preshader : BaseShader<PreshaderOpcode>
{
    private const uint header_fxlc = 'F' | 'X' << 8 | 'L' << 16 | 'C' << 24;
    private const uint header_clit = 'C' | 'L' << 8 | 'I' << 16 | 'T' << 24; // lol
    private const uint header_prsi = 'P' | 'R' << 8 | 'S' << 16 | 'I' << 24;

    public static Preshader ReadPreshader(EffectReader reader)
    {
        var shader = new Preshader
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

    protected override OpcodeData<PreshaderOpcode> MakeRegularToken(BinaryReader reader, uint tokenType, uint length)
    {
        throw new System.NotImplementedException();
    }

    protected override OpcodeData<PreshaderOpcode> MakeComment(uint tokenType, uint length, byte[] readBytes)
    {
        return new OpcodeData<PreshaderOpcode>
        {
            Type = (PreshaderOpcode)tokenType,
            Length = length,
            Comment = Encoding.ASCII.GetString(readBytes),
        };
    }
}
