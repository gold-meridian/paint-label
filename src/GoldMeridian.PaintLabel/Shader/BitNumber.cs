using System;
using System.Runtime.CompilerServices;

namespace GoldMeridian.PaintLabel;

public record struct BitNumber(uint Value)
{
    public bool this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ((Value >> index) & 1) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var mask = 1u << index;
            if (value)
            {
                Value |= mask;
            }
            else
            {
                Value &= ~mask;
            }
        }
    }

    public uint this[int start, int end]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var length = end - start + 1;
            var mask = length == 32 ? 0xFFFFFFFFu : (1u << length) - 1u;
            return (Value >> start) & mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            var length = end - start + 1;
            var mask = length == 32 ? 0xFFFFFFFFu : (1u << length) - 1u;
            
            mask <<= start;
            Value = Value & ~mask | (value << start) & mask;
        }
    }

    public override string ToString()
    {
        return $"0b{Convert.ToString(Value, 2).PadLeft(32, '0')}";
    }
}
