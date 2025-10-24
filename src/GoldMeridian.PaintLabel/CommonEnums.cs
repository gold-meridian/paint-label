namespace GoldMeridian.PaintLabel;

public enum HlslZBufferType
{
    False,
    True,
    UseW,
}

public enum HlslFillMode
{
    Point = 1,
    Wireframe = 2,
    Solid = 3,
}

public enum HlslShadeMode
{
    Flat = 1,
    Gouraund = 2,
    Phong = 3,
}

public enum HlslBlendMode
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

public enum HlslCullMode
{
    None = 1,
    Cw = 2,
    Ccw = 3,
}

public enum HlslCompareFunc
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

public enum HlslFogMode
{
    None,
    Exp,
    Exp2,
    Linear,
}

public enum HlslStencilOp
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

public enum HlslMaterialColorSource
{
    Material,
    Color1,
    Color2,
}

public enum HlslVertexBlendFlags
{
    Disable = 0,
    OneWeights = 1,
    TwoWeights = 2,
    ThreeWeights = 3,
    Tweening = 255,
    ZeroWeights = 256,
}

public enum HlslPatchedEdgeStyle
{
    Discrete,
    Continuous,
}

public enum HlslDebugMonitorTokens
{
    Enable,
    Disable,
}

public enum HlslBlendOp
{
    Add = 1,
    Subtract = 2,
    RevSubtract = 3,
    Min = 4,
    Max = 5,
}

public enum HlslDegreeType
{
    Linear = 1,
    Quadratic = 2,
    Cubic = 3,
    Quintic = 4,
}

//

public enum HlslSamplerStateType
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

public enum HlslTextureAddress
{
    Wrap = 1,
    Mirror = 2,
    Clamp = 3,
    Border = 4,
    MirrorOnce = 5,
}

public enum HlslTextureFilterType
{
    None,
    Point,
    Linear,
    Anisotropic,
    PyramidalQuad,
    GaussianQuad,
    ConvolutionMono,
}
