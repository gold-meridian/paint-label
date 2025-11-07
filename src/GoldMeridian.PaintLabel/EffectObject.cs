using System.Diagnostics.CodeAnalysis;

namespace GoldMeridian.PaintLabel;

public abstract record EffectObjectValue;

public sealed record EffectShader(
    Shader Value
) : EffectObjectValue;

public sealed record EffectSamplerMap(
    string? Name
) : EffectObjectValue;

public sealed record EffectString(
    string? String
) : EffectObjectValue;

public sealed record EffectTexture : EffectObjectValue;

public sealed class EffectObject(
    SymbolType type,
    EffectObjectValue? value
)
{
    public SymbolType Type { get; set; } = type;

    public EffectObjectValue? Value { get; set; } = value;

    public bool TryGetShader(
        [NotNullWhen(returnValue: true)] out EffectShader? value
    )
    {
        value = Value as EffectShader;
        return value is not null;
    }

    public bool TryGetSamplerMap(
        [NotNullWhen(returnValue: true)] out EffectSamplerMap? value
    )
    {
        value = Value as EffectSamplerMap;
        return value is not null;
    }

    public bool TryGetString(
        [NotNullWhen(returnValue: true)] out EffectString? value
    )
    {
        value = Value as EffectString;
        return value is not null;
    }

    public bool TryGetTexture(
        [NotNullWhen(returnValue: true)] out EffectTexture? value
    )
    {
        value = Value as EffectTexture;
        return value is not null;
    }
}
