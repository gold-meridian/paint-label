namespace GoldMeridian.PaintLabel;

public readonly record struct Effect(
    EffectParameter[] Parameters,
    EffectTechnique[] Techniques,
    EffectObject[] Objects,
    Error[] Errors
)
{
    public bool HasErrors => Errors.Length > 0;
}
