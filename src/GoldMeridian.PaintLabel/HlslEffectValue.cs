using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace GoldMeridian.PaintLabel;

public readonly record struct HlslEffectValue(
    string Name,
    string Semantic,
    HlslSymbolTypeInfo Type,
    HlslEffectValue.ValuesUnion Values
)
{
#region Values union
    public abstract record ValuesUnion(ValuesKind Kind, object Array)
    {
        public bool TryGetIntArray(
            [NotNullWhen(returnValue: true)] out int[]? array
        )
        {
            if (Kind != ValuesKind.IntArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is IntArrayValues);

            array = (this as IntArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetFloatArray(
            [NotNullWhen(returnValue: true)] out float[]? array
        )
        {
            if (Kind != ValuesKind.FloatArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is FloatArrayValues);

            array = (this as FloatArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetZBufferTypeArray(
            [NotNullWhen(returnValue: true)] out HlslZBufferType[]? array
        )
        {
            if (Kind != ValuesKind.ZBufferTypeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is ZBufferTypeArrayValues);

            array = (this as ZBufferTypeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetFillModeArray(
            [NotNullWhen(returnValue: true)] out HlslFillMode[]? array
        )
        {
            if (Kind != ValuesKind.FillModeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is FillModeArrayValues);

            array = (this as FillModeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetShadeModeArray(
            [NotNullWhen(returnValue: true)] out HlslShadeMode[]? array
        )
        {
            if (Kind != ValuesKind.ShadeModeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is ShadeModeArrayValues);

            array = (this as ShadeModeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetBlendModeArray(
            [NotNullWhen(returnValue: true)] out HlslBlendMode[]? array
        )
        {
            if (Kind != ValuesKind.BlendModeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is BlendModeArrayValues);

            array = (this as BlendModeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetCullModeArray(
            [NotNullWhen(returnValue: true)] out HlslCullMode[]? array
        )
        {
            if (Kind != ValuesKind.CullModeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is CullModeArrayValues);

            array = (this as CullModeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetCompareFuncArray(
            [NotNullWhen(returnValue: true)] out HlslCompareFunc[]? array
        )
        {
            if (Kind != ValuesKind.CompareFuncArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is CompareFuncArrayValues);

            array = (this as CompareFuncArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetFogModeArray(
            [NotNullWhen(returnValue: true)] out HlslFogMode[]? array
        )
        {
            if (Kind != ValuesKind.FogModeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is FogModeArrayValues);

            array = (this as FogModeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetStencilOpArray(
            [NotNullWhen(returnValue: true)] out HlslStencilOp[]? array
        )
        {
            if (Kind != ValuesKind.StencilOpArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is StencilOpArrayValues);

            array = (this as StencilOpArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetMaterialColorSourceArray(
            [NotNullWhen(returnValue: true)] out HlslMaterialColorSource[]? array
        )
        {
            if (Kind != ValuesKind.MaterialColorSourceArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is MaterialColorSourceArrayValues);

            array = (this as MaterialColorSourceArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetVertexBlendFlagsArray(
            [NotNullWhen(returnValue: true)] out HlslVertexBlendFlags[]? array
        )
        {
            if (Kind != ValuesKind.VertexBlendFlagsArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is VertexBlendFlagsArrayValues);

            array = (this as VertexBlendFlagsArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetPatchedEdgeStyleArray(
            [NotNullWhen(returnValue: true)] out HlslPatchedEdgeStyle[]? array
        )
        {
            if (Kind != ValuesKind.PatchedEdgeStyleArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is PatchedEdgeStyleArrayValues);

            array = (this as PatchedEdgeStyleArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetDebugMonitorTokensArray(
            [NotNullWhen(returnValue: true)] out HlslDebugMonitorTokens[]? array
        )
        {
            if (Kind != ValuesKind.DebugMonitorTokensArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is DebugMonitorTokensArrayValues);

            array = (this as DebugMonitorTokensArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetBlendOpArray(
            [NotNullWhen(returnValue: true)] out HlslBlendOp[]? array
        )
        {
            if (Kind != ValuesKind.BlendOpArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is BlendOpArrayValues);

            array = (this as BlendOpArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetDegreeTypeArray(
            [NotNullWhen(returnValue: true)] out HlslDegreeType[]? array
        )
        {
            if (Kind != ValuesKind.DegreeTypeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is DegreeTypeArrayValues);

            array = (this as DegreeTypeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetTextureAddressArray(
            [NotNullWhen(returnValue: true)] out HlslTextureAddress[]? array
        )
        {
            if (Kind != ValuesKind.TextureAddressArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is TextureAddressArrayValues);

            array = (this as TextureAddressArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetTextureFilterTypeArray(
            [NotNullWhen(returnValue: true)] out HlslTextureFilterType[]? array
        )
        {
            if (Kind != ValuesKind.TextureFilterTypeArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is TextureFilterTypeArrayValues);

            array = (this as TextureFilterTypeArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }

        public bool TryGetEffectSamplerStateArray(
            [NotNullWhen(returnValue: true)] out HlslEffectSamplerState[]? array
        )
        {
            if (Kind != ValuesKind.EffectSamplerStateArray)
            {
                array = null;
                return false;
            }

            Debug.Assert(this is EffectSamplerStateArrayValues);

            array = (this as EffectSamplerStateArrayValues)?.RealValues;
            {
                Debug.Assert(array is not null);
            }

            return array is not null;
        }
    }

    public sealed record IntArrayValues(
        int[] RealValues
    ) : ValuesUnion(ValuesKind.IntArray, RealValues);

    public sealed record FloatArrayValues(
        float[] RealValues
    ) : ValuesUnion(ValuesKind.FloatArray, RealValues);

    public sealed record ZBufferTypeArrayValues(
        HlslZBufferType[] RealValues
    ) : ValuesUnion(ValuesKind.ZBufferTypeArray, RealValues);

    public sealed record FillModeArrayValues(
        HlslFillMode[] RealValues
    ) : ValuesUnion(ValuesKind.FillModeArray, RealValues);

    public sealed record ShadeModeArrayValues(
        HlslShadeMode[] RealValues
    ) : ValuesUnion(ValuesKind.ShadeModeArray, RealValues);

    public sealed record BlendModeArrayValues(
        HlslBlendMode[] RealValues
    ) : ValuesUnion(ValuesKind.BlendModeArray, RealValues);

    public sealed record CullModeArrayValues(
        HlslCullMode[] RealValues
    ) : ValuesUnion(ValuesKind.CullModeArray, RealValues);

    public sealed record CompareFuncArrayValues(
        HlslCompareFunc[] RealValues
    ) : ValuesUnion(ValuesKind.CompareFuncArray, RealValues);

    public sealed record FogModeArrayValues(
        HlslFogMode[] RealValues
    ) : ValuesUnion(ValuesKind.FogModeArray, RealValues);

    public sealed record StencilOpArrayValues(
        HlslStencilOp[] RealValues
    ) : ValuesUnion(ValuesKind.StencilOpArray, RealValues);

    public sealed record MaterialColorSourceArrayValues(
        HlslMaterialColorSource[] RealValues
    ) : ValuesUnion(ValuesKind.MaterialColorSourceArray, RealValues);

    public sealed record VertexBlendFlagsArrayValues(
        HlslVertexBlendFlags[] RealValues
    ) : ValuesUnion(ValuesKind.VertexBlendFlagsArray, RealValues);

    public sealed record PatchedEdgeStyleArrayValues(
        HlslPatchedEdgeStyle[] RealValues
    ) : ValuesUnion(ValuesKind.PatchedEdgeStyleArray, RealValues);

    public sealed record DebugMonitorTokensArrayValues(
        HlslDebugMonitorTokens[] RealValues
    ) : ValuesUnion(ValuesKind.DebugMonitorTokensArray, RealValues);

    public sealed record BlendOpArrayValues(
        HlslBlendOp[] RealValues
    ) : ValuesUnion(ValuesKind.BlendOpArray, RealValues);

    public sealed record DegreeTypeArrayValues(
        HlslDegreeType[] RealValues
    ) : ValuesUnion(ValuesKind.DegreeTypeArray, RealValues);

    public sealed record TextureAddressArrayValues(
        HlslTextureAddress[] RealValues
    ) : ValuesUnion(ValuesKind.TextureAddressArray, RealValues);

    public sealed record TextureFilterTypeArrayValues(
        HlslTextureFilterType[] RealValues
    ) : ValuesUnion(ValuesKind.TextureFilterTypeArray, RealValues);

    public sealed record EffectSamplerStateArrayValues(
        HlslEffectSamplerState[] RealValues
    ) : ValuesUnion(ValuesKind.EffectSamplerStateArray, RealValues);

    public enum ValuesKind : byte
    {
        IntArray,
        FloatArray,
        ZBufferTypeArray,
        FillModeArray,
        ShadeModeArray,
        BlendModeArray,
        CullModeArray,
        CompareFuncArray,
        FogModeArray,
        StencilOpArray,
        MaterialColorSourceArray,
        VertexBlendFlagsArray,
        PatchedEdgeStyleArray,
        DebugMonitorTokensArray,
        BlendOpArray,
        DegreeTypeArray,
        TextureAddressArray,
        TextureFilterTypeArray,
        EffectSamplerStateArray,
    }
#endregion
}
