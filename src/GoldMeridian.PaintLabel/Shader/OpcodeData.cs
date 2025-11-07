using System;

namespace GoldMeridian.PaintLabel;

public sealed class OpcodeData<TKind>
    where TKind : struct, Enum
{
    public TKind Type { get; set; }

    public uint Length { get; set; }

    // ASCII?
    public string? Comment { get; set; }

    public DestinationParameter? Destination { get; set; }

    public SourceParameter[] Sources { get; set; } = [];

    public float[] Constants { get; set; } = [];

    public uint? Extra { get; set; }
}
