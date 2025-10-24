namespace GoldMeridian.PaintLabel.Shader;

public enum RegisterType
{
    Temp = 0,
    Input = 1,
    Const = 2,
    Address = 3,
    RastOut = 4,
    AttrOut = 5,
    Output = 6,
    ConstInt = 7,
    ColorOut = 8,
    DepthOut = 9,
    Sampler = 10,
    Const2 = 11,
    Const3 = 12,
    Const4 = 13,
    ConstBool = 14,
    Loop = 15,
    TempFloat16 = 16,
    MiscType = 17,
    Label = 18,
    Predicate = 19,

    // From ShaderDecompiler

    // Assigned manually
    Texcrdout = 100,
    Texture = 101,
		

    // Preshader register types
    PreshaderLiteral = 200,
    PreshaderInput = 201,
    PreshaderTemp = 202,
}
