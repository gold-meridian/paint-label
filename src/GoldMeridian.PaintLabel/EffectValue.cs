using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GoldMeridian.PaintLabel;

public readonly record struct EffectValue(
    string? Name,
    string? Semantic,
    SymbolTypeInfo Type,
    ValuesUnion Values
);

public sealed class ValuesUnion
{
    private byte[]? oneByteArray;
    private int[]? fourByteArray;
    private EffectSamplerState[]? samplerStateArray;

    public ValuesUnion(byte[] array)
    {
        oneByteArray = array;
    }

    public ValuesUnion(int[] array)
    {
        fourByteArray = array;
    }

    public ValuesUnion(EffectSamplerState[] array)
    {
        samplerStateArray = array;
    }

    // because we're stuck with netstandard2.0, I have to do this in the
    // god-awful manner possible.  would love to just bitcast and return an
    // ienumrable<T> or what-have-you, but this is what we're dealing with.
    public unsafe bool TryGetArray<T>([NotNullWhen(returnValue: true)] out T[]? array)
    {
        if (typeof(T) == typeof(EffectSamplerState))
        {
            array = (samplerStateArray as T[])!;
            return true;
        }

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        if (sizeof(T) == sizeof(int))
        {
            if (fourByteArray is null)
            {
                array = null;
                return false;
            }

            array = Reinterpret<int, T>(fourByteArray);
            return true;
        }

        if (sizeof(T) == sizeof(byte))
        {
            if (oneByteArray is null)
            {
                array = null;
                return false;
            }

            array = Reinterpret<byte, T>(oneByteArray);
            return true;
        }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

        Debug.Fail("TryGetArray with wrong size: " + typeof(T).Name);
        array = null;
        return false;
    }

    public static ValuesUnion FromArray(EffectSamplerState[] array)
    {
        return new ValuesUnion(array);
    }

    public static unsafe ValuesUnion FromArray<T>(T[] array)
        where T : unmanaged
    {
        if (typeof(T) == typeof(int))
        {
            return new ValuesUnion((array as int[])!);
        }

        if (sizeof(T) == sizeof(int))
        {
            return new ValuesUnion(Reinterpret<T, int>(array));
        }

        if (sizeof(T) == sizeof(byte))
        {
            return new ValuesUnion(Reinterpret<T, byte>(array));
        }

        throw new InvalidOperationException("Cannot initialize effect values union with type:" + typeof(T).Name);
    }

    private static TTarget[] Reinterpret<TSource, TTarget>(TSource[] source)
    {
        var targetBuffer = new TTarget[source.Length];

        var sourceSize = Buffer.ByteLength(source);

        // Should be checked ahead-of-time by the caller.
        Debug.Assert(sourceSize == Buffer.ByteLength(targetBuffer));

        Buffer.BlockCopy(source, 0, targetBuffer, 0, source.Length);
        return targetBuffer;
    }
}
