namespace GoldMeridian.PaintLabel;

public readonly record struct EffectParameter(
    EffectValue Value,
    EffectAnnotation[] Annotations
);

public readonly record struct EffectAnnotation(
    EffectValue Value
);
