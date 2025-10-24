namespace GoldMeridian.PaintLabel.Shader;

public enum SourceMod
{
    None,
    Negate,
    Bias,
    BiasNegate,
    Sign,
    SignNegate,
    Complement,
    X2, // Double
    X2Negate, // Double Negate
    Dz, // Divide by Z
    Dw, // Divide by W
    Abs,
    AbsNegate,
    Not,
}
