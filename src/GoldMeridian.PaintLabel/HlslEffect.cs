namespace GoldMeridian.PaintLabel;

public readonly record struct HlslEffect(
    HlslEffectParameter[] Parameters,
    HlslEffectTechnique[] Techniques,
    HlslEffectObject[] Objects,
    HlslError[] Errors
)
{
    public bool HasErrors => Errors.Length > 0;
}
