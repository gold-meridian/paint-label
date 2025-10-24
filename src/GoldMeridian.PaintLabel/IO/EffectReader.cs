using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.IO;

// TODO: hide this object and just expose static methods for reading

public sealed class EffectReader
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
        get => (int)Reader.BaseStream.Position;
        set => Reader.BaseStream.Position = value;
    }

    // Length left to read, not length of the entire buffer.  Yes.  I know.
    public int Length => (int)Reader.BaseStream.Length - Position;

    public BinaryReader Reader { get; }

    private int baseOffset;
    private List<HlslError> errors = [];

    private EffectReader(BinaryReader reader)
    {
        Reader = reader;
    }

    public static HlslEffect ReadEffect(BinaryReader reader)
    {
        var effectReader = new EffectReader(reader);
        return effectReader.ReadEffect();
    }

    private HlslEffect ReadEffect()
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
            var skip = Reader.ReadUInt32() - 8;
            Position += (int)skip;

            ReadVersionToken(out magic, out major, out minor);
        }

        if (!(magic == 0xFEFF && major == 0x09 && minor == 0x01))
        {
            return error_not_an_effect;
        }

        var offset = Reader.ReadUInt32();
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

        var parameters = new HlslEffectParameter[Reader.ReadUInt32()];
        var techniques = new HlslEffectTechnique[Reader.ReadUInt32()];
        _ = Reader.ReadUInt32(); // FIXME: what is this?
        var objects = new HlslEffectObject[Reader.ReadUInt32()];
        for (var i = 0; i < objects.Length; i++)
        {
            objects[i] = new HlslEffectObject(HlslSymbolType.Void, null);
        }

        ReadParameters(parameters, objects);
        ReadTechniques(techniques, objects);

        if (Length < 8)
        {
            return error_unexpected_eof;
        }

        var smallObjectCount = Reader.ReadUInt32();
        var largeObjectCount = Reader.ReadUInt32();

        ReadSmallObjects(smallObjectCount, parameters, techniques, objects);
        ReadLargeObjects(smallObjectCount, largeObjectCount, parameters, techniques, objects);

        return new HlslEffect(
            Parameters: parameters,
            Techniques: techniques,
            Objects: objects,
            Errors: errors.ToArray()
        );
    }

    private void ReadVersionToken(out ushort magic, out byte versionMajor, out byte versionMinor)
    {
        var token = Reader.ReadUInt32();

        magic = (ushort)((token >> 16) & 0xFFFF);
        versionMajor = (byte)((token >> 8) & 0xFF);
        versionMinor = (byte)(token & 0xFF);
    }

    private void ReadParameters(HlslEffectParameter[] parameters, HlslEffectObject[] objects)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var typeOffset = Reader.ReadUInt32();
            var valueOffset = Reader.ReadUInt32();
            _ = Reader.ReadUInt32(); // flags
            var annotations = new HlslEffectAnnotation[Reader.ReadUInt32()];

            ReadAnnotations(annotations, objects);
            var effectValue = ReadValue(typeOffset, valueOffset, objects);

            parameters[i] = new HlslEffectParameter(
                Value: effectValue,
                Annotations: annotations
            );
        }
    }

    private void ReadTechniques(HlslEffectTechnique[] techniques, HlslEffectObject[] objects)
    {
        for (var i = 0; i < techniques.Length; i++)
        {
            var nameOffset = Reader.ReadUInt32();
            var annotations = new HlslEffectAnnotation[Reader.ReadUInt32()];
            var passes = new HlslEffectPass[Reader.ReadUInt32()];

            var name = ReadStringAtPosition(nameOffset);
            ReadAnnotations(annotations, objects);
            ReadPasses(passes, objects);

            techniques[i] = new HlslEffectTechnique(
                name,
                passes,
                annotations
            );
        }
    }

    private void ReadAnnotations(HlslEffectAnnotation[] annotations, HlslEffectObject[] objects)
    {
        if (annotations.Length == 0)
        {
            return;
        }

        for (var i = 0; i < annotations.Length; i++)
        {
            var typeOffset = Reader.ReadUInt32();
            var valueOffset = Reader.ReadUInt32();

            var effectValue = ReadValue(typeOffset, valueOffset, objects);

            annotations[i] = new HlslEffectAnnotation(
                Value: effectValue
            );
        }
    }

    private void ReadPasses(HlslEffectPass[] passes, HlslEffectObject[] objects)
    {
        for (var i = 0; i < passes.Length; i++)
        {
            var nameOffset = Reader.ReadUInt32();
            var annotations = new HlslEffectAnnotation[Reader.ReadUInt32()];
            var states = new HlslEffectState[Reader.ReadUInt32()];

            var name = ReadStringAtPosition(nameOffset);
            ReadAnnotations(annotations, objects);
            ReadStates(states, objects);

            passes[i] = new HlslEffectPass(
                name,
                states,
                annotations
            );
        }
    }

    private void ReadStates(HlslEffectState[] states, HlslEffectObject[] objects)
    {
        for (var i = 0; i < states.Length; i++)
        {
            var type = Reader.ReadUInt32();
            _ = Reader.ReadUInt32(); // FIXME
            var typeOffset = Reader.ReadUInt32();
            var valueOffset = Reader.ReadUInt32();

            var stateType = (HlslRenderStateType)type;
            var effect = ReadValue(typeOffset, valueOffset, objects);

            states[i] = new HlslEffectState(
                stateType,
                effect
            );
        }
    }

    private void ReadSmallObjects(uint smallObjectCount, HlslEffectParameter[] parameters, HlslEffectTechnique[] techniques, HlslEffectObject[] objects)
    {
        if (smallObjectCount == 0)
        {
            return;
        }

        for (var i = 1; i < smallObjectCount + 1; i++)
        {
            var index = Reader.ReadUInt32();
            var length = Reader.ReadUInt32();

            var obj = objects[index];
            if (obj.Type is HlslSymbolType.String)
            {
                using (KeepPos())
                {
                    if (length > 0)
                    {
                        var value = ReadString(length);
                        obj.Value = new HlslEffectString(value);
                    }
                }
            }
            else if (
                obj.Type is HlslSymbolType.Texture
                         or HlslSymbolType.Texture1D
                         or HlslSymbolType.Texture2D
                         or HlslSymbolType.Texture3D
                         or HlslSymbolType.TextureCube
                         or HlslSymbolType.Sampler
                         or HlslSymbolType.Sampler1D
                         or HlslSymbolType.Sampler2D
                         or HlslSymbolType.Sampler3D
                         or HlslSymbolType.SamplerCube
            )
            {
                using (KeepPos())
                {
                    if (length > 0)
                    {
                        var name = ReadString(length);
                        obj.Value = new HlslEffectSamplerMap(name);
                    }
                }
            }
            else if (obj.Type is HlslSymbolType.PixelShader or HlslSymbolType.VertexShader)
            {
                var shader = Shader.Shader.ReadShader(this);
                if (shader is null)
                {
                    return;
                }

                obj.Value = new HlslEffectShader(shader);
            }
            else
            {
                throw new InvalidOperationException($"Unknown small object type: {obj.Type}");
            }

            var blockLength = length + 3 - (length - 1) % 4;
            Position += (int)blockLength;
        }
    }

    private void ReadLargeObjects(uint smallObjectCount, uint largeObjectCount, HlslEffectParameter[] parameters, HlslEffectTechnique[] techniques, HlslEffectObject[] objects)
    {
        if (largeObjectCount == 0)
        {
            return;
        }

        var objectCount = smallObjectCount + largeObjectCount + 1;
        for (var i = smallObjectCount + 1; i < objectCount; i++)
        {
            var technique = Reader.ReadInt32();
            var index = Reader.ReadUInt32();
            _ = Reader.ReadUInt32(); // FIXME
            var state = Reader.ReadUInt32();
            var type = Reader.ReadUInt32();
            var length = Reader.ReadUInt32();

            uint objectIndex;
            if (technique == -1)
            {
                var values = parameters[index].Value.Values;
                if (!values.TryGetArray<HlslEffectSamplerState>(out var samplerStates))
                {
                    throw new InvalidOperationException($"No HlslEffectSamplerState[] for LO {i} index {index} (technique: {technique}, state: {state})");
                }

                if (!samplerStates[state].Value.Values.TryGetArray<int>(out var ints))
                {
                    throw new InvalidOperationException($"No int[] for HlslEffectSamplerState in LO {i} index {index} (technique: {technique}, state: {state})");
                }

                objectIndex = (uint)ints[0];
            }
            else
            {
                var values = techniques[technique].Passes[index].States[state].Value.Values;
                if (!values.TryGetArray<int>(out var ints))
                {
                    throw new InvalidOperationException($"No int[] for LO {i} index {index} (technique: {technique})");
                }

                objectIndex = (uint)ints[0];
            }

            var obj = objects[objectIndex];
            if (obj.Type is HlslSymbolType.PixelShader or HlslSymbolType.VertexShader)
            {
                if (type == 2)
                {
                    // Standalone preshader, exists only for effect passes that
                    // do not use a single vertex/fragment shader.

                    throw new NotImplementedException();
                }
                else
                {
                    var shader = Shader.Shader.ReadShader(this);
                    if (shader is null)
                    {
                        return;
                    }

                    obj.Value = new HlslEffectShader(shader);
                }
            }
            else if (
                obj.Type is HlslSymbolType.Texture
                         or HlslSymbolType.Texture1D
                         or HlslSymbolType.Texture2D
                         or HlslSymbolType.Texture3D
                         or HlslSymbolType.TextureCube
                         or HlslSymbolType.Sampler
                         or HlslSymbolType.Sampler1D
                         or HlslSymbolType.Sampler2D
                         or HlslSymbolType.Sampler3D
                         or HlslSymbolType.SamplerCube
            )
            {
                if (length > 0)
                {
                    var name = ReadString(length);
                    obj.Value = new HlslEffectSamplerMap(name);
                }
            }
            else if (obj.Type is not HlslSymbolType.Void) // TODO: why?
            {
                throw new InvalidOperationException($"Unknown large object type: {obj.Type}");
            }

            var blockLength = length + 3 - (length - 1) % 4;
            Position += (int)blockLength;
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
            SeekFromOffset(typeOffset);

            var type = Reader.ReadUInt32();
            var valueClass = Reader.ReadUInt32();
            var nameOffset = Reader.ReadUInt32();
            var semanticOffset = Reader.ReadUInt32();
            var elementCount = Reader.ReadUInt32();

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

                var columnCount = Reader.ReadUInt32();
                var rowCount = Reader.ReadUInt32();

                SeekFromOffset(valueOffset);

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
                            ints[i] = Reader.ReadInt32();
                        }

                        values = ValuesUnion.FromArray(ints);
                        break;

                    case HlslSymbolType.Float:
                        var floats = new float[valueCount];
                        for (var i = 0; i < floats.Length; i++)
                        {
                            floats[i] = Reader.ReadSingle();
                        }

                        values = ValuesUnion.FromArray(floats);
                        break;

                    case HlslSymbolType.Bool:
                        var booleans = new bool[valueCount];
                        for (var i = 0; i < booleans.Length; i++)
                        {
                            booleans[i] = Reader.ReadBoolean();
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

                SeekFromOffset(valueOffset);

                if (
                    parameterType is HlslSymbolType.Sampler
                                  or HlslSymbolType.Sampler1D
                                  or HlslSymbolType.Sampler2D
                                  or HlslSymbolType.Sampler3D
                                  or HlslSymbolType.SamplerCube
                )
                {
                    var states = new HlslEffectSamplerState[Reader.ReadUInt32()];
                    for (var i = 0; i < states.Length; i++)
                    {
                        var samplerType = (HlslSamplerStateType)(Reader.ReadUInt32() & ~0xA0);
                        _ = Reader.ReadUInt32(); // FIXME
                        var stateTypeOffset = Reader.ReadUInt32();
                        var stateValueOffset = Reader.ReadUInt32();

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
                        ints[i] = Reader.ReadInt32();
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
                var members = new HlslSymbolStructMember[Reader.ReadUInt32()];

                var structSize = 0u;
                for (var i = 0; i < members.Length; i++)
                {
                    var memberParameterType = (HlslSymbolType)Reader.ReadUInt32();
                    var memberParameterClass = (HlslSymbolClass)Reader.ReadUInt32();

                    var memberNameOffset = Reader.ReadUInt32();
                    _ = Reader.ReadUInt32(); // memberSemanticOffset

                    var memberName = ReadStringAtPosition(memberNameOffset);

                    var memberElementCount = Reader.ReadUInt32();
                    var memberColumnCount = Reader.ReadUInt32();
                    var memberRowCount = Reader.ReadUInt32();

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
                var values = new float[valueCount];
                var ii = 0;
                do
                {
                    foreach (var member in members)
                    {
                        var amountToRead = member.Info.Rows * member.Info.Elements;
                        var sizeToRead = member.Info.Columns << 2;

                        for (var k = 0; k < amountToRead; k++)
                        {
                            var buf = Reader.ReadBytes((int)sizeToRead);
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
        var strPtr = Reader.ReadUInt32();
        return ReadStringAtPosition(strPtr);
    }

    private string? ReadStringAtPosition(uint pos)
    {
        if (pos == 0 || baseOffset + pos >= Reader.BaseStream.Length)
        {
            return null;
        }

        using (KeepPos())
        {
            SeekFromOffset(pos);

            var length = Reader.ReadUInt32();
            return ReadString(length);
        }
    }

    private string? ReadString(uint length)
    {
        if (length == 0)
        {
            return null;
        }

        var bytes = Reader.ReadBytes((int)length - 1);
        return Encoding.ASCII.GetString(bytes);
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
