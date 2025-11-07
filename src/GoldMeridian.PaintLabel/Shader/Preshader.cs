using System;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel;

public sealed class Preshader : BaseShader<PreshaderOpcode>
{
    private const uint header_fxlc = 'F' | 'X' << 8 | 'L' << 16 | 'C' << 24;
    private const uint header_clit = 'C' | 'L' << 8 | 'I' << 16 | 'T' << 24; // lol
    private const uint header_prsi = 'P' | 'R' << 8 | 'S' << 16 | 'I' << 24;

    public double[] Literals { get; set; } = [];

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

    private void ReadClit(BinaryReader reader)
    {
        Literals = new double[reader.ReadUInt32()];

        for (var i = 0; i < Literals.Length; i++)
        {
            Literals[i] = reader.ReadDouble();
        }
    }

    private void ReadFxlc(BinaryReader reader)
    {
        var opcodeCount = reader.ReadUInt32();

        for (var i = 0; i < opcodeCount; i++)
        {
            ReadPreshaderOpcode(reader);
        }
    }

    private void ReadPrsi(BinaryReader reader)
    {
        // TODO
    }

    private void ReadPreshaderOpcode(BinaryReader reader)
    {
        var opToken = reader.ReadUInt32();

        var type = ((opToken >> 16) & 0xFFFF) switch
        {
            0x1000 => PreshaderOpcode.Mov,
            0x1010 => PreshaderOpcode.Neg,
            0x1030 => PreshaderOpcode.Rcp,
            0x1040 => PreshaderOpcode.Frc,
            0x1050 => PreshaderOpcode.Exp,
            0x1060 => PreshaderOpcode.Log,
            0x1070 => PreshaderOpcode.Rsq,
            0x1080 => PreshaderOpcode.Sin,
            0x1090 => PreshaderOpcode.Cos,
            0x10A0 => PreshaderOpcode.Asin,
            0x10B0 => PreshaderOpcode.Acos,
            0x10C0 => PreshaderOpcode.Atan,
            0x2000 => PreshaderOpcode.Min,
            0x2010 => PreshaderOpcode.Max,
            0x2020 => PreshaderOpcode.Lt,
            0x2030 => PreshaderOpcode.Ge,
            0x2040 => PreshaderOpcode.Add,
            0x2050 => PreshaderOpcode.Mul,
            0x2060 => PreshaderOpcode.Atan2,
            0x2080 => PreshaderOpcode.Div,
            0x3000 => PreshaderOpcode.Cmp,
            0x3010 => PreshaderOpcode.MovC,
            0x5000 => PreshaderOpcode.Dot,
            0x5020 => PreshaderOpcode.Noise,
            0xA000 => PreshaderOpcode.MinScalar,
            0xA010 => PreshaderOpcode.MaxScalar,
            0xA020 => PreshaderOpcode.LtScalar,
            0xA030 => PreshaderOpcode.GeScalar,
            0xA040 => PreshaderOpcode.AddScalar,
            0xA050 => PreshaderOpcode.MulScalar,
            0xA060 => PreshaderOpcode.Atan2Scalar,
            0xA080 => PreshaderOpcode.DivScalar,
            0xD000 => PreshaderOpcode.DotScalar,
            0xD020 => PreshaderOpcode.NoiseScalar,
            _ => throw new InvalidOperationException($"Unknown preshader opcode: {(opToken >> 16) & 0xFFFF} ({opToken})")
        };

        var length = reader.ReadUInt32();
        var opcode = new OpcodeData<PreshaderOpcode>
        {
            Type = type,
            Length = length + 1,
            Sources = new SourceParameter[length],
        };

        // elements = opToken & 0xFF;

        for (var i = 0; i < opcode.Length; i++)
        {
            var operandArrayCount = reader.ReadUInt32();
            var operandType = reader.ReadUInt32();
            var operandItem = reader.ReadUInt32();

            var actualRegister = operandItem / 4;
            var swizzle = (Swizzle)(operandItem % 4);

            if (operandType == 1)
            {
                swizzle = Swizzle.X;
                actualRegister = operandItem;
            }

            OpcodeParameter param;
            if (i == opcode.Length - 1)
            {
                opcode.Destination = new DestinationParameter
                {
                    WriteX = swizzle == Swizzle.X,
                    WriteY = swizzle == Swizzle.Y,
                    WriteZ = swizzle == Swizzle.Z,
                    WriteW = swizzle == Swizzle.W,
                };
                param = opcode.Destination;
            }
            else
            {
                opcode.Sources[i] = new SourceParameter
                {
                    SwizzleX = swizzle,
                    SwizzleY = swizzle,
                    SwizzleZ = swizzle,
                    SwizzleW = swizzle
                };
                param = opcode.Sources[i];
            }

            param.Register = actualRegister;

            switch (operandType)
            {
                case 1:
                    param.RegisterType = RegisterType.PreshaderLiteral;
                    break;

                case 2:
                    param.RegisterType = RegisterType.PreshaderInput;
                    for (var j = 0; j < operandArrayCount; j++)
                    {
                        reader.ReadUInt32();
                        reader.ReadUInt32();
                    }

                    break;

                case 4:
                    param.RegisterType = RegisterType.Const;
                    break;

                case 7:
                    param.RegisterType = RegisterType.PreshaderTemp;
                    break;

                default:
                    throw new InvalidOperationException($"Unhandled preshader operand type: {operandType}");
            }
        }

        Opcodes.Add(opcode);
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
