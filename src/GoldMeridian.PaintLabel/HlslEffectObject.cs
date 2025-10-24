using System.Diagnostics.CodeAnalysis;

namespace GoldMeridian.PaintLabel;

public abstract record HlslEffectObjectValue;

public sealed record HlslEffectShader(
    object Value
) : HlslEffectObjectValue;

public sealed record HlslEffectSamplerMap(
    string? Name
) : HlslEffectObjectValue;

public sealed record HlslEffectString(
    string? String
) : HlslEffectObjectValue;

public sealed record HlslEffectTexture : HlslEffectObjectValue;

public sealed class HlslEffectObject(
    HlslSymbolType type,
    HlslEffectObjectValue? value
)
{
    public HlslSymbolType Type { get; set; } = type;

    public HlslEffectObjectValue? Value { get; set; } = value;

    public bool TryGetShader(
        [NotNullWhen(returnValue: true)] out HlslEffectShader? value
    )
    {
        value = Value as HlslEffectShader;
        return value is not null;
    }

    public bool TryGetSamplerMap(
        [NotNullWhen(returnValue: true)] out HlslEffectSamplerMap? value
    )
    {
        value = Value as HlslEffectSamplerMap;
        return value is not null;
    }

    public bool TryGetString(
        [NotNullWhen(returnValue: true)] out HlslEffectString? value
    )
    {
        value = Value as HlslEffectString;
        return value is not null;
    }

    public bool TryGetTexture(
        [NotNullWhen(returnValue: true)] out HlslEffectTexture? value
    )
    {
        value = Value as HlslEffectTexture;
        return value is not null;
    }
}
