using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GoldMeridian.PaintLabel.IO;

// TODO: hide this object and just expose static methods for reading

public sealed class EffectReader
{
    public int Position
    {
        get => (int)Reader.BaseStream.Position;
        set => Reader.BaseStream.Position = value;
    }

    // Length left to read, not length of the entire buffer.  Yes.  I know.
    public int Length => (int)Reader.BaseStream.Length - Position;

    public BinaryReader Reader { get; }

    private int baseOffset;

    private EffectReader(BinaryReader reader)
    {
        Reader = reader;
    }

    public static Effect ReadEffect(BinaryReader reader)
    {
        var effectReader = new EffectReader(reader);
        return effectReader.ReadEffect();
    }

    private Effect ReadEffect()
    {
        if (Length < 8)
        {
            throw new EndOfStreamException();
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
            throw new InvalidDataException("Not an Effects Framework binary");
        }

        var offset = Reader.ReadUInt32();
        baseOffset = Position;
        if (offset > Length)
        {
            throw new EndOfStreamException();
        }

        Position += (int)offset;

        if (Length < 16)
        {
            throw new EndOfStreamException();
        }

        var parameters = new EffectParameter[Reader.ReadUInt32()];
        var techniques = new EffectTechnique[Reader.ReadUInt32()];
        _ = Reader.ReadUInt32(); // FIXME: what is this?
        var objects = new EffectObject[Reader.ReadUInt32()];
        for (var i = 0; i < objects.Length; i++)
        {
            objects[i] = new EffectObject(SymbolType.Void, null);
        }

        ReadParameters(parameters, objects);
        ReadTechniques(techniques, objects);

        if (Length < 8)
        {
            throw new EndOfStreamException();
        }

        var smallObjectCount = Reader.ReadUInt32();
        var largeObjectCount = Reader.ReadUInt32();

        ReadSmallObjects(smallObjectCount, parameters, techniques, objects);
        ReadLargeObjects(smallObjectCount, largeObjectCount, parameters, techniques, objects);

        return new Effect(
            Parameters: parameters,
            Techniques: techniques,
            Objects: objects
        );
    }

    private void ReadVersionToken(out ushort magic, out byte versionMajor, out byte versionMinor)
    {
        var token = Reader.ReadUInt32();

        magic = (ushort)((token >> 16) & 0xFFFF);
        versionMajor = (byte)((token >> 8) & 0xFF);
        versionMinor = (byte)(token & 0xFF);
    }

    private void ReadParameters(EffectParameter[] parameters, EffectObject[] objects)
    {
        for (var i = 0; i < parameters.Length; i++)
        {
            var typeOffset = Reader.ReadUInt32();
            var valueOffset = Reader.ReadUInt32();
            _ = Reader.ReadUInt32(); // flags
            var annotations = new EffectAnnotation[Reader.ReadUInt32()];

            ReadAnnotations(annotations, objects);
            var effectValue = ReadValue(typeOffset, valueOffset, objects);

            parameters[i] = new EffectParameter(
                Value: effectValue,
                Annotations: annotations
            );
        }
    }

    private void ReadTechniques(EffectTechnique[] techniques, EffectObject[] objects)
    {
        for (var i = 0; i < techniques.Length; i++)
        {
            var nameOffset = Reader.ReadUInt32();
            var annotations = new EffectAnnotation[Reader.ReadUInt32()];
            var passes = new EffectPass[Reader.ReadUInt32()];

            var name = ReadStringAtPosition(nameOffset);
            ReadAnnotations(annotations, objects);
            ReadPasses(passes, objects);

            techniques[i] = new EffectTechnique(
                name,
                passes,
                annotations
            );
        }
    }

    private void ReadAnnotations(EffectAnnotation[] annotations, EffectObject[] objects)
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

            annotations[i] = new EffectAnnotation(
                Value: effectValue
            );
        }
    }

    private void ReadPasses(EffectPass[] passes, EffectObject[] objects)
    {
        for (var i = 0; i < passes.Length; i++)
        {
            var nameOffset = Reader.ReadUInt32();
            var annotations = new EffectAnnotation[Reader.ReadUInt32()];
            var states = new EffectState[Reader.ReadUInt32()];

            var name = ReadStringAtPosition(nameOffset);
            ReadAnnotations(annotations, objects);
            ReadStates(states, objects);

            passes[i] = new EffectPass(
                name,
                states,
                annotations
            );
        }
    }

    private void ReadStates(EffectState[] states, EffectObject[] objects)
    {
        for (var i = 0; i < states.Length; i++)
        {
            var type = Reader.ReadUInt32();
            _ = Reader.ReadUInt32(); // FIXME
            var typeOffset = Reader.ReadUInt32();
            var valueOffset = Reader.ReadUInt32();

            var stateType = (RenderStateType)type;
            var effect = ReadValue(typeOffset, valueOffset, objects);

            states[i] = new EffectState(
                stateType,
                effect
            );
        }
    }

    private void ReadSmallObjects(uint smallObjectCount, EffectParameter[] parameters, EffectTechnique[] techniques, EffectObject[] objects)
    {
        if (smallObjectCount == 0)
        {
            return;
        }

        for (var i = 1; i < smallObjectCount + 1; i++)
        {
            var index = Reader.ReadUInt32();
            var length = Reader.ReadUInt32();

            var pos = PushPos();
            try
            {
                var obj = objects[index];
                if (obj.Type is SymbolType.String)
                {
                    if (length > 0)
                    {
                        var value = ReadString(length);
                        obj.Value = new EffectString(value);
                    }
                }
                else if (
                    obj.Type is SymbolType.Texture
                             or SymbolType.Texture1D
                             or SymbolType.Texture2D
                             or SymbolType.Texture3D
                             or SymbolType.TextureCube
                             or SymbolType.Sampler
                             or SymbolType.Sampler1D
                             or SymbolType.Sampler2D
                             or SymbolType.Sampler3D
                             or SymbolType.SamplerCube
                )
                {
                    if (length > 0)
                    {
                        var name = ReadString(length);
                        obj.Value = new EffectSamplerMap(name);
                    }
                }
                else if (obj.Type is SymbolType.PixelShader or SymbolType.VertexShader)
                {
                    var shader = Shader.ReadShader(Reader);
                    obj.Value = new EffectShader(shader);
                }
                else
                {
                    throw new InvalidOperationException($"Unknown small object type: {obj.Type}");
                }
            }
            finally
            {
                PopPos(pos);
            }

            var blockLength = length + 3 - (length - 1) % 4;
            Position += (int)blockLength;
        }
    }

    private void ReadLargeObjects(uint smallObjectCount, uint largeObjectCount, EffectParameter[] parameters, EffectTechnique[] techniques, EffectObject[] objects)
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
                if (!values.TryGetArray<EffectSamplerState>(out var samplerStates))
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

            var pos = PushPos();
            try
            {
                var obj = objects[objectIndex];
                if (obj.Type is SymbolType.PixelShader or SymbolType.VertexShader)
                {
                    if (type == 2)
                    {
                        // Standalone preshader, exists only for effect passes that
                        // do not use a single vertex/fragment shader.

                        throw new NotImplementedException();
                    }
                    else
                    {
                        var shader = Shader.ReadShader(Reader);
                        obj.Value = new EffectShader(shader);
                    }
                }
                else if (
                    obj.Type is SymbolType.Texture
                             or SymbolType.Texture1D
                             or SymbolType.Texture2D
                             or SymbolType.Texture3D
                             or SymbolType.TextureCube
                             or SymbolType.Sampler
                             or SymbolType.Sampler1D
                             or SymbolType.Sampler2D
                             or SymbolType.Sampler3D
                             or SymbolType.SamplerCube
                )
                {
                    if (length > 0)
                    {
                        var name = ReadString(length);
                        obj.Value = new EffectSamplerMap(name);
                    }
                }
                else if (obj.Type is not SymbolType.Void) // TODO: why?
                {
                    throw new InvalidOperationException($"Unknown large object type: {obj.Type}");
                }
            }
            finally
            {
                PopPos(pos);
            }

            var blockLength = length + 3 - (length - 1) % 4;
            Position += (int)blockLength;
        }
    }

    private EffectValue ReadValue(
        uint typeOffset,
        uint valueOffset,
        EffectObject[] objects
    )
    {
        var pos = PushPos();
        try
        {
            SeekFromOffset(typeOffset);

            var type = Reader.ReadUInt32();
            var valueClass = Reader.ReadUInt32();
            var nameOffset = Reader.ReadUInt32();
            var semanticOffset = Reader.ReadUInt32();
            var elementCount = Reader.ReadUInt32();

            var parameterType = (SymbolType)type;
            var parameterClass = (SymbolClass)valueClass;
            var name = ReadStringAtPosition(nameOffset);
            var semantic = ReadStringAtPosition(semanticOffset);

            Debug.Assert(parameterClass is >= SymbolClass.Scalar and <= SymbolClass.Struct);

            if (
                parameterClass is SymbolClass.Scalar
                               or SymbolClass.Vector
                               or SymbolClass.MatrixRows
                               or SymbolClass.MatrixColumns
            )
            {
                Debug.Assert(parameterType is >= SymbolType.Bool and <= SymbolType.Float);

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
                    case SymbolType.Int:
                        var ints = new int[valueCount];
                        for (var i = 0; i < ints.Length; i++)
                        {
                            ints[i] = Reader.ReadInt32();
                        }

                        values = ValuesUnion.FromArray(ints);
                        break;

                    case SymbolType.Float:
                        var floats = new float[valueCount];
                        for (var i = 0; i < floats.Length; i++)
                        {
                            floats[i] = Reader.ReadSingle();
                        }

                        values = ValuesUnion.FromArray(floats);
                        break;

                    case SymbolType.Bool:
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

                return new EffectValue(
                    name,
                    semantic,
                    new SymbolTypeInfo(
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

            if (parameterClass is SymbolClass.Object)
            {
                Debug.Assert(parameterType is >= SymbolType.String and <= SymbolType.VertexShader);

                SeekFromOffset(valueOffset);

                if (
                    parameterType is SymbolType.Sampler
                                  or SymbolType.Sampler1D
                                  or SymbolType.Sampler2D
                                  or SymbolType.Sampler3D
                                  or SymbolType.SamplerCube
                )
                {
                    var states = new EffectSamplerState[Reader.ReadUInt32()];
                    for (var i = 0; i < states.Length; i++)
                    {
                        var samplerType = (SamplerStateType)(Reader.ReadUInt32() & ~0xA0);
                        _ = Reader.ReadUInt32(); // FIXME
                        var stateTypeOffset = Reader.ReadUInt32();
                        var stateValueOffset = Reader.ReadUInt32();

                        var stateValue = ReadValue(stateTypeOffset, stateValueOffset, objects);

                        states[i] = new EffectSamplerState(
                            Type: samplerType,
                            Value: stateValue
                        );

                        if (samplerType == SamplerStateType.Texture)
                        {
                            if (!stateValue.Values.TryGetArray<int>(out var ints))
                            {
                                throw new InvalidOperationException("Sampler type was Texture but values was not int[]");
                            }

                            objects[ints[0]].Type = parameterType;
                        }
                    }

                    return new EffectValue(
                        name,
                        semantic,
                        new SymbolTypeInfo(
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

                    return new EffectValue(
                        name,
                        semantic,
                        new SymbolTypeInfo(
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

            if (parameterClass is SymbolClass.Struct)
            {
                var members = new SymbolStructMember[Reader.ReadUInt32()];

                var structSize = 0u;
                for (var i = 0; i < members.Length; i++)
                {
                    var memberParameterType = (SymbolType)Reader.ReadUInt32();
                    var memberParameterClass = (SymbolClass)Reader.ReadUInt32();

                    var memberNameOffset = Reader.ReadUInt32();
                    _ = Reader.ReadUInt32(); // memberSemanticOffset

                    var memberName = ReadStringAtPosition(memberNameOffset);

                    var memberElementCount = Reader.ReadUInt32();
                    var memberColumnCount = Reader.ReadUInt32();
                    var memberRowCount = Reader.ReadUInt32();

                    // TODO: nested structs
                    Debug.Assert(memberParameterClass is >= SymbolClass.Scalar and <= SymbolClass.MatrixColumns);
                    Debug.Assert(memberParameterType is >= SymbolType.Bool and <= SymbolType.Float);

                    var memSize = 4 * memberRowCount;
                    if (memberElementCount > 0)
                    {
                        memSize *= memberElementCount;
                    }

                    structSize += memSize;

                    members[i] = new SymbolStructMember(
                        memberName,
                        new SymbolTypeInfo(
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

                return new EffectValue(
                    name,
                    semantic,
                    new SymbolTypeInfo(
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
        finally
        {
            PopPos(pos);
        }
    }

    private string? ReadStringAndOffset()
    {
        var strPtr = Reader.ReadUInt32();
        return ReadStringAtPosition(strPtr);
    }

    private string? ReadStringAtPosition(uint position)
    {
        if (position == 0 || baseOffset + position >= Reader.BaseStream.Length)
        {
            return null;
        }

        var pos = PushPos();
        try
        {
            SeekFromOffset(position);

            var length = Reader.ReadUInt32();
            return ReadString(length);
        }
        finally
        {
            PopPos(pos);
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

    private int PushPos()
    {
        return Position;
    }

    private void PopPos(int position)
    {
        Position = position;
    }

    private void SeekFromOffset(uint fromOffset)
    {
        Position = baseOffset + (int)fromOffset;
    }
}
