namespace GoldMeridian.PaintLabel;

public readonly record struct HlslEffectParameter(
    HlslEffectValue Value,
    HlslEffectAnnotation[] Annotations
);

public readonly record struct HlslEffectAnnotation(
    HlslEffectValue Value
);
