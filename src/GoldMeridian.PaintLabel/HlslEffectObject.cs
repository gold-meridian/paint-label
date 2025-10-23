using System.Diagnostics.CodeAnalysis;

namespace GoldMeridian.PaintLabel;

public abstract record HlslEffectObjectValue(
    HlslSymbolType Type
);

public sealed record HlslEffectShader(
    HlslSymbolType Type,
    object Value
) : HlslEffectObjectValue(Type);

public sealed record HlslEffectSamplerMap(
    HlslSymbolType Type,
    string Name
) : HlslEffectObjectValue(Type);

public sealed record HlslEffectString(
    HlslSymbolType Type,
    string String
) : HlslEffectObjectValue(Type);

public sealed record HlslEffectTexture(
    HlslSymbolType Type
) : HlslEffectObjectValue(Type);

public record struct HlslEffectObject(
    HlslSymbolType Type,
    HlslEffectObjectValue Object
)
{
    public HlslSymbolType Type { get; set; } = Type;

    public bool TryGetShader(
        [NotNullWhen(returnValue: true)] out HlslEffectShader? value
    )
    {
        value = Object as HlslEffectShader;
        return value is not null;
    }

    public bool TryGetSamplerMap(
        [NotNullWhen(returnValue: true)] out HlslEffectSamplerMap? value
    )
    {
        value = Object as HlslEffectSamplerMap;
        return value is not null;
    }

    public bool TryGetString(
        [NotNullWhen(returnValue: true)] out HlslEffectString? value
    )
    {
        value = Object as HlslEffectString;
        return value is not null;
    }

    public bool TryGetTexture(
        [NotNullWhen(returnValue: true)] out HlslEffectTexture? value
    )
    {
        value = Object as HlslEffectTexture;
        return value is not null;
    }
}
