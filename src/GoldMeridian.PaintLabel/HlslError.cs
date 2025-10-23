namespace GoldMeridian.PaintLabel;

/// <summary>
///     The special-case positions of errors in <see cref="HlslError"/>.
/// </summary>
public enum HlslErrorPosition : sbyte
{
    /// <summary>
    ///     There is no error.
    /// </summary>
    None = -3,

    /// <summary>
    ///     An error has occurred before processing.
    /// </summary>
    Before = -2,

    /// <summary>
    ///     An error has occurred after processing.
    /// </summary>
    After = -1,

    /// <summary>
    ///     Default; byte offset of the error in the bytecode.
    /// </summary>
    ByteOffset = 0,
}

/// <summary>
///     The location of an error.
/// </summary>
/// <param name="PositionKind">The kind of position.</param>
/// <param name="Position">
///     The byte offset in a bytecode file if <see cref="PositionKind"/> is
///     <see cref="HlslErrorPosition.ByteOffset"/>.
/// </param>
public readonly record struct HlslErrorLocation(
    HlslErrorPosition PositionKind,
    int Position
)
{
    public static readonly HlslErrorLocation NONE = new(HlslErrorPosition.None, 0);

    public static readonly HlslErrorLocation BEFORE = new(HlslErrorPosition.Before, 0);

    public static readonly HlslErrorLocation AFTER = new(HlslErrorPosition.After, 0);
}

public readonly record struct HlslError(
    string? Message,
    string? FileName,
    HlslErrorLocation Location
);
