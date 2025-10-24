using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.Shader;

public sealed class Shader : BaseShader<ShaderOpcode>
{
    private const uint header_pres = 'P' | 'R' << 8 | 'E' << 16 | 'S' << 24;

    public Preshader? Preshader { get; set; }

    public static Shader ReadShader(BinaryReader reader)
    {
        var shader = new Shader
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
            
            case header_pres:
                Preshader = Preshader.ReadPreshader(reader);
                return true;
        }

        return false;
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
