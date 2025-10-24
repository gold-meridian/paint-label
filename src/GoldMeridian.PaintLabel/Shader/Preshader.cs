using System;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.Shader;

public sealed class Preshader : BaseShader<PreshaderOpcode>
{
    private const uint header_fxlc = 'F' | 'X' << 8 | 'L' << 16 | 'C' << 24;
    private const uint header_clit = 'C' | 'L' << 8 | 'I' << 16 | 'T' << 24; // lol
    private const uint header_prsi = 'P' | 'R' << 8 | 'S' << 16 | 'I' << 24;

    public static Preshader ReadPreshader(BinaryReader reader)
    {
        var shader = new Preshader
        {
            Version = ShaderVersion.Read(reader),
        };

        while (shader.ReadOpcode(reader)) { }

        return shader;
    }

    protected override bool ProcessSpecialComments(BinaryReader reader, uint length)
    {
        var header = reader.ReadUInt32();
        switch (header)
        {
            case HEADER_CTAB:
                ReadConstantTable(reader, length - 4);
                return true;
            
            case header_clit:
                ReadClit(reader);
                return true;
            
            case header_fxlc:
                ReadFxlc(reader);
                return true;
            
            case header_prsi:
                ReadPrsi(reader);
                return true;
        }

        return false;
    }

    protected override OpcodeData<PreshaderOpcode> MakeRegularToken(BinaryReader reader, uint tokenType, uint length)
    {
        throw new InvalidOperationException("Preshader does not support regular tokens");
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
