using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.Shader;

public abstract class BaseShader<TKind>
    where TKind : struct, Enum
{
    public const uint HEADER_CTAB = 'C' | 'T' << 8 | 'A' << 16 | 'B' << 24;

    public const uint TOKEN_COMMENT = 0xFFFE;
    public const uint TOKEN_END = 0xFFFF;

    public required ShaderVersion Version { get; init; }

    public List<Constant> Constants { get; } = [];

    public List<OpcodeData<TKind>> Opcodes { get; } = [];

    public Dictionary<uint, SymbolTypeInfo> Types { get; } = [];

    public string? Creator { get; private set; }

    public string? Target { get; private set; }

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

    protected void ReadConstantTable(BinaryReader reader, uint length)
    {
        var start = (uint)reader.BaseStream.Position;

        var ctabLength = reader.ReadUInt32();
        if (ctabLength != 28)
        {
            throw new InvalidOperationException($"Invalid CTAB length: {ctabLength}");
        }

        Creator = ReadString(reader, start, length);

        var version = ShaderVersion.Read(reader);
        if (Version != version)
        {
            throw new InvalidOperationException($"Wrong CTAB version, got \"{version}\" (but expected \"{Version}\")");
        }

        var constantsCount = reader.ReadUInt32();
        var constantsInfo = reader.ReadUInt32();

        Target = ReadString(reader, start, length);

        for (var i = 0; i < constantsCount; i++)
        {
            reader.BaseStream.Position = start + constantsInfo + i * 20;

            var constant = new Constant
            {
                Name = ReadString(reader, start, length),
                RegSet = (SymbolRegisterSet)reader.ReadUInt16(),
                RegIndex = reader.ReadUInt16(),
                RegCount = reader.ReadUInt16(),
            };

            _ = reader.ReadUInt16();

            constant.TypeInfo = ReadTypeInfo(reader, start, length);

            var defaultValue = reader.ReadUInt32();
            if (defaultValue > 0)
            {
                reader.BaseStream.Position = start + defaultValue;
                var typeSize = constant.TypeInfo.Value.GetSize(out var typeActualSize);

                constant.DefaultValue = new float[typeActualSize];
                for (var j = 0u; j < typeSize; j++)
                {
                    var value = reader.ReadSingle();

                    if (constant.TypeInfo.Value.TransformTypeDefaultValueDataPos(j, out var arrayPos))
                    {
                        constant.DefaultValue[arrayPos] = value;
                    }
                }
            }

            Constants.Add(constant);
        }

        Constants.Sort((a, b) => a.RegIndex - b.RegIndex);
    }

    protected string? ReadString(BinaryReader reader, uint start, uint length)
    {
        var position = reader.ReadUInt32();

        var streamPos = reader.BaseStream.Position;
        position += start;

        if (position >= start + length)
        {
            return null;
        }

        try
        {
            reader.BaseStream.Seek(position, SeekOrigin.Begin);

            var strLength = 0;
            while (position + strLength < start + length)
            {
                if (reader.ReadByte() == 0)
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    return Encoding.ASCII.GetString(reader.ReadBytes(strLength));
                }
                else if (reader.BaseStream.Position >= reader.BaseStream.Length)
                {
                    return null;
                }

                strLength++;
            }

            return null;
        }
        finally
        {
            reader.BaseStream.Seek(streamPos, SeekOrigin.Begin);
        }
    }

    protected SymbolTypeInfo ReadTypeInfo(BinaryReader reader, uint start, uint length)
    {
        var position = reader.ReadUInt32();

        if (Types.TryGetValue(position, out var cachedType))
        {
            return cachedType;
        }

        var streamPos = reader.BaseStream.Position;

        try
        {
            reader.BaseStream.Seek(start + position, SeekOrigin.Begin);

            var info = new SymbolTypeInfo(
                ParameterClass: (SymbolClass)reader.ReadUInt16(),
                ParameterType: (SymbolType)reader.ReadUInt16(),
                Rows: reader.ReadUInt16(),
                Columns: reader.ReadUInt16(),
                Elements: reader.ReadUInt16(),
                Members: new SymbolStructMember[reader.ReadUInt16()]
            );

            if (info.Members.Length > 0)
            {
                var membersPos = reader.ReadUInt32();
                reader.BaseStream.Seek(start + membersPos, SeekOrigin.Begin);

                for (var i = 0; i < info.Members.Length; i++)
                {
                    info.Members[i] = new SymbolStructMember(
                        Name: ReadString(reader, start, length),
                        Info: ReadTypeInfo(reader, start, length)
                    );
                }
            }

            Types[position] = info;

            return info;
        }
        finally
        {
            reader.BaseStream.Seek(streamPos, SeekOrigin.Begin);
        }
    }

    protected abstract bool ProcessSpecialComments(BinaryReader reader, uint length);

    protected abstract OpcodeData<TKind> MakeRegularToken(BinaryReader reader, uint tokenType, uint length);

    protected abstract OpcodeData<TKind> MakeComment(uint tokenType, uint length, byte[] readBytes);
}
