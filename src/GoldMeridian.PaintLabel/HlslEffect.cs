namespace GoldMeridian.PaintLabel;

public readonly record struct HlslEffect(
    HlslEffectParameter[] Parameters,
    HlslEffectTechnique[] Techniques,
    HlslError[] Errors
)
{
    public bool HasErrors => Errors.Length > 0;
}
