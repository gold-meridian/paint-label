using System;

namespace GoldMeridian.PaintLabel;

public enum SymbolClass
{
    Scalar,
    Vector,
    MatrixRows,
    MatrixColumns,
    Object,
    Struct,
}

public enum SymbolType
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

public readonly record struct SymbolStructMember(
    string? Name,
    SymbolTypeInfo Info
);

public readonly record struct SymbolTypeInfo(
    SymbolClass ParameterClass,
    SymbolType ParameterType,
    uint Rows,
    uint Columns,
    uint Elements,
    SymbolStructMember[] Members
)
{
    public uint GetSize(out uint actualSize)
    {
        switch (ParameterClass)
        {
            case SymbolClass.Scalar:
                actualSize = 1;
                return 1;

            case SymbolClass.Vector:
                actualSize = Columns;
                return Columns;

            case SymbolClass.MatrixRows:
                actualSize = Rows * Columns;
                return 4 * Rows;

            case SymbolClass.MatrixColumns:
                actualSize = Rows * Columns;
                return 4 * Columns;

            case SymbolClass.Object:
                actualSize = 0;
                return 0;

            case SymbolClass.Struct:
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
            case SymbolClass.Scalar:
                arrayPos = 0;
                return dataPos == 0;

            case SymbolClass.Vector:
                arrayPos = dataPos;
                return dataPos < Columns;

            case SymbolClass.MatrixRows:
            case SymbolClass.MatrixColumns:
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

            case SymbolClass.Object:
                return false;

            case SymbolClass.Struct:
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
