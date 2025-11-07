namespace GoldMeridian.PaintLabel;

/// <summary>
///     The special-case positions of errors in <see cref="Error"/>.
/// </summary>
public enum ErrorPosition : sbyte
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
///     <see cref="ErrorPosition.ByteOffset"/>.
/// </param>
public readonly record struct ErrorLocation(
    ErrorPosition PositionKind,
    int Position
)
{
    public static readonly ErrorLocation NONE = new(ErrorPosition.None, 0);

    public static readonly ErrorLocation BEFORE = new(ErrorPosition.Before, 0);

    public static readonly ErrorLocation AFTER = new(ErrorPosition.After, 0);
}

public readonly record struct Error(
    string? Message,
    string? FileName,
    ErrorLocation Location
);
