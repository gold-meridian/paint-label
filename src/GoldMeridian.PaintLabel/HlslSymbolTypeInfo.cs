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
);
