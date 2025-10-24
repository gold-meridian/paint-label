using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.IO;

// TODO: hide this object and just expose static methods for reading

public sealed class EffectReader(BinaryReader reader)
{
    private readonly struct DisposeAction(Action action) : IDisposable
    {
        public void Dispose()
        {
            action();
        }
    }

    private static readonly HlslEffect error_unexpected_eof = new(
        Parameters: [],
        Techniques: [],
        Objects: [],
        Errors:
        [
            new HlslError("Unexpected EOF", null, HlslErrorLocation.NONE),
        ]
    );

    private static readonly HlslEffect error_not_an_effect = new(
        Parameters: [],
        Techniques: [],
        Objects: [],
        Errors:
        [
            new HlslError("Not an Effects Framework binary", null, HlslErrorLocation.NONE),
        ]
    );

    public int Position
    {
        get => (int)reader.BaseStream.Position;
        set => reader.BaseStream.Position = value;
    }

    // Length left to read, not length of the entire buffer.  Yes.  I know.
    public int Length => (int)reader.BaseStream.Length - Position;

    private int baseOffset;
    private List<HlslError> errors = [];

    public HlslEffect ReadEffect()
    {
        if (Length < 8)
        {
            return error_unexpected_eof;
        }

        ReadVersionToken(out var magic, out var major, out var minor);
        if (magic == 0xBCF0 && major == 0x0B && minor == 0xCF)
        {
            // The XNA4-provided Effect compiler adds extra data we need to
            // skip.
            var skip = reader.ReadUInt32() - 8;
            Position += (int)skip;

            ReadVersionToken(out magic, out major, out minor);
        }

        if (!(magic == 0xFEFF && major == 0x09 && minor == 0x01))
        {
            return error_not_an_effect;
        }

        var offset = reader.ReadUInt32();
        baseOffset = Position;
        if (offset > Length)
        {
            return error_unexpected_eof;
        }

        Position += (int)offset;

        if (Length < 16)
        {
            return error_unexpected_eof;
        }

        var parameters = new HlslEffectParameter[reader.ReadUInt32()];
        var techniques = new HlslEffectTechnique[reader.ReadUInt32()];
        _ = reader.ReadUInt32(); // FIXME: what is this?
        var objects = new HlslEffectObject[reader.ReadUInt32()];

        ReadParameters(parameters, objects);
        ReadTechniques(techniques, objects);

        if (Length < 8)
        {
            return error_unexpected_eof;
        }

        return new HlslEffect(
            Parameters: parameters,
            Techniques: techniques,
            Objects: objects,
            Errors: errors.ToArray()
        );
    }

    private void ReadVersionToken(out ushort magic, out byte versionMajor, out byte versionMinor)
    {
        var token = reader.ReadUInt32();

        magic = (ushort)((token >> 16) & 0xFFFF);
        versionMajor = (byte)((token >> 8) & 0xFF);
        versionMinor = (byte)(token & 0xFF);
    }

    private void ReadParameters(HlslEffectParameter[] parameters, HlslEffectObject[] objects)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var typeOffset = reader.ReadUInt32();
            var valueOffset = reader.ReadUInt32();
            _ = reader.ReadUInt32(); // flags
            var annotations = new HlslEffectAnnotation[reader.ReadUInt32()];

            ReadAnnotations(annotations, objects);
            var effectValue = ReadValue(typeOffset, valueOffset, objects);
        }
    }

    private void ReadTechniques(HlslEffectTechnique[] parameters, HlslEffectObject[] objects) { }

    private void ReadAnnotations(HlslEffectAnnotation[] annotations, HlslEffectObject[] objects)
    {
        if (annotations.Length == 0)
        {
            return;
        }

        for (var i = 0; i < annotations.Length; i++)
        {
            var typeOffset = reader.ReadUInt32();
            var valueOffset = reader.ReadUInt32();

            var effectValue = ReadValue(typeOffset, valueOffset, objects);

            annotations[i] = new HlslEffectAnnotation(
                Value: effectValue
            );
        }
    }

    private HlslEffectValue ReadValue(
        uint typeOffset,
        uint valueOffset,
        HlslEffectObject[] objects
    )
    {
        using (KeepPos())
        {
            Position = baseOffset + (int)typeOffset;

            var type = reader.ReadUInt32();
            var valueClass = reader.ReadUInt32();
            var nameOffset = reader.ReadUInt32();
            var semanticOffset = reader.ReadUInt32();
            var elementCount = reader.ReadUInt32();

            var parameterType = (HlslSymbolType)type;
            var parameterClass = (HlslSymbolClass)valueClass;
            var name = ReadStringAtPosition(nameOffset);
            var semantic = ReadStringAtPosition(semanticOffset);

            Debug.Assert(parameterClass is >= HlslSymbolClass.Scalar and <= HlslSymbolClass.Struct);

            if (
                parameterClass is HlslSymbolClass.Scalar
                               or HlslSymbolClass.Vector
                               or HlslSymbolClass.MatrixRows
                               or HlslSymbolClass.MatrixColumns
            )
            {
                Debug.Assert(parameterType is >= HlslSymbolType.Bool and <= HlslSymbolType.Float);

                var columnCount = reader.ReadUInt32();
                var rowCount = reader.ReadUInt32();

                Position = baseOffset + (int)valueOffset;

                var valueCount = columnCount * rowCount;
                if (elementCount > 0)
                {
                    valueCount *= elementCount;
                }

                ValuesUnion values;
                switch (parameterType)
                {
                    case HlslSymbolType.Int:
                        var ints = new int[valueCount];
                        for (var i = 0; i < ints.Length; i++)
                        {
                            ints[i] = reader.ReadInt32();
                        }

                        values = ValuesUnion.FromArray(ints);
                        break;

                    case HlslSymbolType.Float:
                        var floats = new float[valueCount];
                        for (var i = 0; i < floats.Length; i++)
                        {
                            floats[i] = reader.ReadSingle();
                        }

                        values = ValuesUnion.FromArray(floats);
                        break;

                    case HlslSymbolType.Bool:
                        var booleans = new bool[valueCount];
                        for (var i = 0; i < booleans.Length; i++)
                        {
                            booleans[i] = reader.ReadBoolean();
                        }

                        values = ValuesUnion.FromArray(booleans);
                        break;

                    default:
                        throw new InvalidOperationException($"Cannot read value of scalar: {parameterType}");
                }

                return new HlslEffectValue(
                    name,
                    semantic,
                    new HlslSymbolTypeInfo(
                        parameterClass,
                        parameterType,
                        rowCount,
                        columnCount,
                        elementCount,
                        []
                    ),
                    values
                );
            }

            if (parameterClass is HlslSymbolClass.Object)
            {
                Debug.Assert(parameterType is >= HlslSymbolType.String and <= HlslSymbolType.VertexShader);

                Position = baseOffset + (int)valueOffset;

                if (
                    parameterType is HlslSymbolType.Sampler
                                  or HlslSymbolType.Sampler1D
                                  or HlslSymbolType.Sampler2D
                                  or HlslSymbolType.Sampler3D
                                  or HlslSymbolType.SamplerCube
                )
                {
                    var states = new HlslEffectSamplerState[reader.ReadUInt32()];
                    for (var i = 0; i < states.Length; i++)
                    {
                        var samplerType = (HlslSamplerStateType)(reader.ReadUInt32() & ~0xA0);
                        _ = reader.ReadUInt32(); // FIXME
                        var stateTypeOffset = reader.ReadUInt32();
                        var stateValueOffset = reader.ReadUInt32();

                        var stateValue = ReadValue(stateTypeOffset, stateValueOffset, objects);

                        states[i] = new HlslEffectSamplerState(
                            Type: samplerType,
                            Value: stateValue
                        );

                        if (samplerType == HlslSamplerStateType.Texture)
                        {
                            if (!stateValue.Values.TryGetArray<int>(out var ints))
                            {
                                throw new InvalidOperationException("Sampler type was Texture but values was not int[]");
                            }

                            objects[ints[0]].Type = parameterType;
                        }
                    }

                    return new HlslEffectValue(
                        name,
                        semantic,
                        new HlslSymbolTypeInfo(
                            parameterClass,
                            parameterType,
                            0,
                            0,
                            elementCount,
                            []
                        ),
                        ValuesUnion.FromArray(states)
                    );
                }

                // else
                {
                    var objectCount = 1u;
                    if (elementCount > 0)
                    {
                        objectCount = elementCount;
                    }

                    var ints = new int[objectCount];
                    for (var i = 0; i < ints.Length; i++)
                    {
                        ints[i] = reader.ReadInt32();
                    }

                    var values = ValuesUnion.FromArray(ints);

                    for (var i = 0; i < objectCount; i++)
                    {
                        objects[ints[i]].Type = parameterType;
                    }

                    return new HlslEffectValue(
                        name,
                        semantic,
                        new HlslSymbolTypeInfo(
                            parameterClass,
                            parameterType,
                            0,
                            0,
                            elementCount,
                            []
                        ),
                        values
                    );
                }
            }

            if (parameterClass is HlslSymbolClass.Struct)
            {
                var members = new HlslSymbolStructMember[reader.ReadUInt32()];

                var structSize = 0u;
                for (var i = 0; i < members.Length; i++)
                {
                    var memberParameterType = (HlslSymbolType)reader.ReadUInt32();
                    var memberParameterClass = (HlslSymbolClass)reader.ReadUInt32();

                    var memberNameOffset = reader.ReadUInt32();
                    _ = reader.ReadUInt32(); // memberSemanticOffset

                    var memberName = ReadStringAtPosition(memberNameOffset);

                    var memberElementCount = reader.ReadUInt32();
                    var memberColumnCount = reader.ReadUInt32();
                    var memberRowCount = reader.ReadUInt32();

                    // TODO: nested structs
                    Debug.Assert(memberParameterClass is >= HlslSymbolClass.Scalar and <= HlslSymbolClass.MatrixColumns);
                    Debug.Assert(memberParameterType is >= HlslSymbolType.Bool and <= HlslSymbolType.Float);

                    var memSize = 4 * memberRowCount;
                    if (memberElementCount > 0)
                    {
                        memSize *= memberElementCount;
                    }

                    structSize += memSize;

                    members[i] = new HlslSymbolStructMember(
                        memberName,
                        new HlslSymbolTypeInfo(
                            memberParameterClass,
                            memberParameterType,
                            memberRowCount,
                            memberColumnCount,
                            memberElementCount,
                            []
                        )
                    );
                }

                var columnCount = structSize;
                const int row_count = 1;

                var valueCount = structSize;
                if (elementCount > 0)
                {
                    valueCount *= elementCount;
                }

                var dstOffset = 0;
                float[] values = new float[valueCount];
                var ii = 0;
                do
                {
                    foreach (var member in members)
                    {
                        var amountToRead = member.Info.Rows * member.Info.Elements;
                        var sizeToRead = member.Info.Columns << 2;

                        for (var k = 0; k < amountToRead; k++)
                        {
                            var buf = reader.ReadBytes((int)sizeToRead);
                            Buffer.BlockCopy(buf, 0, values, dstOffset, (int)sizeToRead);
                            dstOffset += 4;
                        }
                    }
                }
                while (++ii < elementCount);

                return new HlslEffectValue(
                    name,
                    semantic,
                    new HlslSymbolTypeInfo(
                        parameterClass,
                        parameterType,
                        row_count,
                        columnCount,
                        elementCount,
                        members
                    ),
                    ValuesUnion.FromArray(values)
                );
            }

            throw new InvalidOperationException($"Somehow unhandled parameter class: {parameterClass}");
        }
    }

    private string? ReadStringAndOffset()
    {
        var strPtr = reader.ReadUInt32();
        return ReadStringAtPosition(strPtr);
    }

    private string? ReadStringAtPosition(uint pos)
    {
        if (pos == 0 || baseOffset + pos >= reader.BaseStream.Length)
        {
            return null;
        }

        using (KeepPos())
        {
            Position = baseOffset + (int)pos;

            var length = reader.ReadUInt32();
            return ReadString(length);
        }
    }

    private string? ReadString(uint length)
    {
        if (length == 0)
        {
            return null;
        }

        return Encoding.ASCII.GetString(reader.ReadBytes((int)length - 1));
    }

    private IDisposable KeepPos()
    {
        var currentPos = Position;
        return new DisposeAction(
            () =>
            {
                Position = currentPos;
            }
        );
    }

    private void SeekFromOffset(uint fromOffset)
    {
        Position = baseOffset + (int)fromOffset;
    }
}
