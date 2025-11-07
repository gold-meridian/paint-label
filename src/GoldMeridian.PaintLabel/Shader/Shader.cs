using System;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel;

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
        var opcode = new OpcodeData<ShaderOpcode>
        {
            Type = (ShaderOpcode)tokenType,
            Length = length,
        };

        if (length <= 0)
        {
            return opcode;
        }

        switch (opcode.Type)
        {
            case ShaderOpcode.Call or ShaderOpcode.CallNz:
                throw new NotImplementedException($"Unhandled opcode: \"{opcode.Type}\" (call or callnz)");

            case ShaderOpcode.Def:
                opcode.Destination = DestinationParameter.Read(reader);
                opcode.Constants = [reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()];
                break;

            case ShaderOpcode.Dcl:
                opcode.Extra = reader.ReadUInt32();
                opcode.Destination = DestinationParameter.Read(reader);
                break;

            default:
            {
                var i = 0;

                if (opcode.Length > i && !OpcodeTypeInfo.Opcodes[opcode.Type].NoDest)
                {
                    opcode.Destination = DestinationParameter.Read(reader);
                    i++;
                }

                if (opcode.Length > i)
                {
                    var start = i;
                    opcode.Sources = new SourceParameter[opcode.Length - i];
                    for (; i < opcode.Length; i++)
                    {
                        opcode.Sources[i - start] = SourceParameter.Read(reader);
                    }
                }

                break;
            }
        }

        return opcode;
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
