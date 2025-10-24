namespace GoldMeridian.PaintLabel.Shader;

public sealed class Constant
{
    public string? Name { get; set; }

    public HlslSymbolRegisterSet RegSet { get; set; }

    public ushort RegIndex { get; set; }

    public ushort RegCount { get; set; }

    public HlslSymbolTypeInfo? TypeInfo { get; set; }

    public float[]? DefaultValue { get; set; }
}
