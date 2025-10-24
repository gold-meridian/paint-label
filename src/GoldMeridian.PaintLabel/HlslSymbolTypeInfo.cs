using System;

namespace GoldMeridian.PaintLabel;

public enum HlslSymbolClass
{
    Scalar,
    Vector,
    MatrixRows,
    MatrixColumns,
    Object,
    Struct,
}

public enum HlslSymbolType
{
    Void,
    Bool,
    Int,
    Float,
    String,
    Texture,
    Texture1D,
    Texture2D,
    Texture3D,
    TextureCube,
    Sampler,
    Sampler1D,
    Sampler2D,
    Sampler3D,
    SamplerCube,
    PixelShader,
    VertexShader,
    PixelFragment,
    VertexFragment,
    Unsupported,
}

public readonly record struct HlslSymbolStructMember(
    string? Name,
    HlslSymbolTypeInfo Info
);

public readonly record struct HlslSymbolTypeInfo(
    HlslSymbolClass ParameterClass,
    HlslSymbolType ParameterType,
    uint Rows,
    uint Columns,
    uint Elements,
    HlslSymbolStructMember[] Members
)
{
    public uint GetSize(out uint actualSize)
    {
        switch (ParameterClass)
        {
            case HlslSymbolClass.Scalar:
                actualSize = 1;
                return 1;

            case HlslSymbolClass.Vector:
                actualSize = Columns;
                return Columns;

            case HlslSymbolClass.MatrixRows:
                actualSize = Rows * Columns;
                return 4 * Rows;

            case HlslSymbolClass.MatrixColumns:
                actualSize = Rows * Columns;
                return 4 * Columns;

            case HlslSymbolClass.Object:
                actualSize = 0;
                return 0;

            case HlslSymbolClass.Struct:
                var size = 0u;
                actualSize = 0;

                foreach (var member in Members)
                {
                    size += Math.Max(4, member.Info.GetSize(out var memberActualSize));
                    actualSize += memberActualSize;
                }

                return size;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool TransformTypeDefaultValueDataPos(uint dataPos, out uint arrayPos)
    {
        arrayPos = 0;

        switch (ParameterClass)
        {
            case HlslSymbolClass.Scalar:
                arrayPos = 0;
                return dataPos == 0;

            case HlslSymbolClass.Vector:
                arrayPos = dataPos;
                return dataPos < Columns;

            case HlslSymbolClass.MatrixRows:
            case HlslSymbolClass.MatrixColumns:
                var row = dataPos % 4;
                if (row >= Rows)
                {
                    return false;
                }

                var col = dataPos / 4;
                if (col >= Columns)
                {
                    return false;
                }

                arrayPos = row * Columns + col;
                return true;

            case HlslSymbolClass.Object:
                return false;

            case HlslSymbolClass.Struct:
                var posOffset = 0u;
                var actualOffset = 0u;

                foreach (var member in Members)
                {
                    var memberSize = Math.Max(4u, member.Info.GetSize(out var memberActualSize));

                    if (memberSize + posOffset > dataPos)
                    {
                        var result = member.Info.TransformTypeDefaultValueDataPos(dataPos - posOffset, out arrayPos);
                        arrayPos += actualOffset;
                        return result;
                    }

                    posOffset += memberSize;
                    actualOffset += memberActualSize;
                }

                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
