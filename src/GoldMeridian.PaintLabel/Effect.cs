namespace GoldMeridian.PaintLabel;

public readonly record struct Effect(
    EffectParameter[] Parameters,
    EffectTechnique[] Techniques,
    EffectObject[] Objects
);

public readonly record struct EffectParameter(
    EffectValue Value,
    EffectAnnotation[] Annotations
);

public readonly record struct EffectAnnotation(
    EffectValue Value
);

public readonly record struct EffectState(
    RenderStateType Type,
    EffectValue Value
);

public readonly record struct EffectPass(
    string? Name,
    EffectState[] States,
    EffectAnnotation[] Annotations
);

public readonly record struct EffectTechnique(
    string? Name,
    EffectPass[] Passes,
    EffectAnnotation[] Annotations
);
