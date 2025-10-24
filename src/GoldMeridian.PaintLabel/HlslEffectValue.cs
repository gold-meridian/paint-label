using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace GoldMeridian.PaintLabel;

public readonly record struct HlslEffectValue(
    string? Name,
    string? Semantic,
    HlslSymbolTypeInfo Type,
    ValuesUnion Values
);

public sealed class ValuesUnion
{
    private byte[]? oneByteArray;
    private int[]? fourByteArray;
    private HlslEffectSamplerState[]? samplerStateArray;

    public ValuesUnion(byte[] array)
    {
        oneByteArray = array;
    }

    public ValuesUnion(int[] array)
    {
        fourByteArray = array;
    }

    public ValuesUnion(HlslEffectSamplerState[] array)
    {
        samplerStateArray = array;
    }

    // because we're stuck with netstandard2.0, I have to do this in the
    // god-awful manner possible.  would love to just bitcast and return an
    // ienumrable<T> or what-have-you, but this is what we're dealing with.
    public unsafe bool TryGetArray<T>([NotNullWhen(returnValue: true)] out T[]? array)
        where T : unmanaged
    {
        if (typeof(T) == typeof(HlslEffectSamplerState))
        {
            array = (samplerStateArray as T[])!;
            return true;
        }

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

        Debug.Fail("TryGetArray with wrong size: " + typeof(T).Name);
        array = null;
        return false;
    }

    public static ValuesUnion FromArray(HlslEffectSamplerState[] array)
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
