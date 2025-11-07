namespace GoldMeridian.PaintLabel;

public enum ZBufferType
{
    False,
    True,
    UseW,
}

public enum FillMode
{
    Point = 1,
    Wireframe = 2,
    Solid = 3,
}

public enum ShadeMode
{
    Flat = 1,
    Gouraund = 2,
    Phong = 3,
}

public enum BlendMode
{
    Zero = 1,
    One = 2,
    SrcColor = 3,
    InvSrcColor = 4,
    SrcAlpha = 5,
    InvSrcAlpha = 6,
    DestAlpha = 7,
    DestColor = 9,
    InvDestColor = 10,
    SrcAlphaSat = 11,
    BothSrcAlpha = 12,
    BothInvSrcAlpha = 13,
    BlendFactor = 14,
    InvBlendFactor = 15,
    SrcColor2 = 16,
    InvSrcColor2 = 17,
}

public enum CullMode
{
    None = 1,
    Cw = 2,
    Ccw = 3,
}

public enum CompareFunc
{
    Never = 1,
    Less = 2,
    Equal = 3,
    LessEqual = 4,
    Greater = 5,
    NotEqual = 6,
    GreaterEqual = 7,
    Always = 8,
}

public enum FogMode
{
    None,
    Exp,
    Exp2,
    Linear,
}

public enum StencilOp
{
    Keep = 1,
    Zero = 2,
    Replace = 3,
    IncrSat = 4,
    DecrSat = 5,
    Invert = 6,
    Incr = 7,
    Decr = 8,
}

public enum MaterialColorSource
{
    Material,
    Color1,
    Color2,
}

public enum VertexBlendFlags
{
    Disable = 0,
    OneWeights = 1,
    TwoWeights = 2,
    ThreeWeights = 3,
    Tweening = 255,
    ZeroWeights = 256,
}

public enum PatchedEdgeStyle
{
    Discrete,
    Continuous,
}

public enum DebugMonitorTokens
{
    Enable,
    Disable,
}

public enum BlendOp
{
    Add = 1,
    Subtract = 2,
    RevSubtract = 3,
    Min = 4,
    Max = 5,
}

public enum DegreeType
{
    Linear = 1,
    Quadratic = 2,
    Cubic = 3,
    Quintic = 4,
}

//

public enum SamplerStateType
{
    Unknown0 = 0,
    Unknown1 = 1,
    Unknown2 = 2,
    Unknown3 = 3,
    Texture = 4,
    AddressU = 5,
    AddressV = 6,
    AddressW = 7,
    BorderColor = 8,
    MagFilter = 9,
    MinFilter = 10,
    MipFilter = 11,
    MipMapLodBias = 12,
    MaxMipLevel = 13,
    MaxAnisotropy = 14,
    SrgbTexture = 15,
    ElementIndex = 16,
    DmapOffset = 17,
}

public enum TextureAddress
{
    Wrap = 1,
    Mirror = 2,
    Clamp = 3,
    Border = 4,
    MirrorOnce = 5,
}

public enum TextureFilterType
{
    None,
    Point,
    Linear,
    Anisotropic,
    PyramidalQuad,
    GaussianQuad,
    ConvolutionMono,
}

//

public enum SymbolRegisterSet
{
    Bool,
    Int4,
    Float4,
    Sampler,
}

//

// Not in order according to D3D.  Uses mojoshader's reordered data:
// - all the Wrap values are in a row, not separate.
public enum RenderStateType
{
    ZEnable,
    FillMode,
    ShadeMode,
    ZWriteEnable,
    AlphaTestEnable,
    LastPixel,
    SrcBlend,
    DestBlend,
    CullMode,
    ZFunc,
    AlphaRef,
    AlphaFunc,
    DitherEnable,
    AlphaBlendEnable,
    FogEnable,
    SpecularEnable,
    FogColor,
    FogTableMode,
    FogStart,
    FogEnd,
    FogDensity,
    RangeFogEnable,
    StencilEnable,
    StencilFail,
    StencilZFail,
    StencilPass,
    StencilFunc,
    StencilRef,
    StencilMask,
    StencilWriteMask,
    TextureFactor,
    Wrap0,
    Wrap1,
    Wrap2,
    Wrap3,
    Wrap4,
    Wrap5,
    Wrap6,
    Wrap7,
    Wrap8,
    Wrap9,
    Wrap10,
    Wrap11,
    Wrap12,
    Wrap13,
    Wrap14,
    Wrap15,
    Clipping,
    Lighting,
    Ambient,
    FogVertexMode,
    ColorVertex,
    LocalViewer,
    NormalizeNormals,
    DiffuseMaterialSource,
    SpecularMaterialSource,
    AmbientMaterialSource,
    EmissiveMaterialSource,
    VertexBlend,
    ClipPlaneEnable,
    PointSize,
    PointSizeMin,
    PointSpriteEnable,
    PointScaleEnable,
    PointScaleA,
    PointScaleB,
    PointScaleC,
    MultiSampleAntiAlias,
    MultiSampleMask,
    PatchEdgeStyle,
    DebugMonitorToken,
    PointSizeMax,
    IndexedVertexBlendEnable,
    ColorWriteEnable,
    TweenFactor,
    BlendOp,
    PositionDegree,
    NormalDegree,
    ScissorTestEnable,
    SlopeScaleDepthBias,
    AntiAliasedLineEnable,
    MinTessellationLevel,
    MaxTessellationLevel,
    AdaptiveTessX,
    AdaptiveTessY,
    AdaptiveTessZ,
    AdaptiveTessW,
    EnableAdaptiveTessellation,
    TwoSidedStencilMode,
    CcwStencilFail,
    CcwStencilZFail,
    CcwStencilPass,
    CcwStencilFunc,
    ColorWriteEnable1,
    ColorWriteEnable2,
    ColorWriteEnable3,
    BlendFactor,
    SrgbWriteEnable,
    DepthBias,
    SeparateAlphaBlendEnable,
    SrcBlendAlpha,
    DestBlendAlpha,
    BlendOpAlpha,

    // mojoshader says these aren't "states", but are numbers that get used by
    // HlslEffectStateType.
    VertexShader = 146,
    PixelShader = 147,
}
