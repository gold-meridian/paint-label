using System;
using System.Collections.Generic;
using System.IO;

namespace GoldMeridian.PaintLabel.Shader;

public abstract class BaseShader<TKind>
    where TKind : struct, Enum
{
    public const uint TOKEN_COMMENT = 0xFFFE;
    public const uint TOKEN_END = 0xFFFF;

    public required ShaderVersion Version { get; init; }

    public List<Constant> Constants { get; } = [];

    public List<OpcodeData<TKind>> Opcodes { get; } = [];

    public Dictionary<uint, HlslSymbolTypeInfo> Types { get; } = [];

    protected bool ReadOpcode(BinaryReader reader)
    {
        var dontAddToken = false;

        var token = new BitNumber(reader.ReadUInt32());
        var tokenType = token[0, 15];

        OpcodeData<TKind> opcode;
        if (tokenType == TOKEN_COMMENT)
        {
            var length = token[16, 30];
            var commentStart = reader.BaseStream.Position;

            if (ProcessSpecialComments(reader, length * 4))
            {
                reader.BaseStream.Position = commentStart + length * 4;

                opcode = new OpcodeData<TKind>(); // unused
                dontAddToken = true;
            }
            else
            {
                reader.BaseStream.Position = commentStart;
                opcode = MakeComment(tokenType, length, reader.ReadBytes((int)length * 4));
            }
        }
        else if (tokenType == TOKEN_END)
        {
            opcode = new OpcodeData<TKind>(); // unused
            dontAddToken = true;
        }
        else
        {
            var length = token[24, 27];
            opcode = MakeRegularToken(reader, tokenType, length);
        }

        if (!dontAddToken)
        {
            Opcodes.Add(opcode);
        }

        return tokenType != TOKEN_END;
    }

    protected abstract bool ProcessSpecialComments(BinaryReader reader, uint length);

    protected abstract OpcodeData<TKind> MakeRegularToken(BinaryReader reader, uint tokenType, uint length);

    protected abstract OpcodeData<TKind> MakeComment(uint tokenType, uint length, byte[] readBytes);
}
