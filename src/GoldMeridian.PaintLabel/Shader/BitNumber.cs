namespace GoldMeridian.PaintLabel.Shader;

public readonly record struct BitNumber(uint Value)
{
    public bool this[int index] => ((Value >> index) & 1) != 0;

    public uint this[int start, int end]
    {
        get
        {
            var length = end - start + 1;
            var mask = 0u;
            for (var i = 0; i < length; i++)
            {
                mask = (mask << 1) | 1;
            }

            mask <<= start;
            return (Value & mask) >> start;
        }
    }
}
