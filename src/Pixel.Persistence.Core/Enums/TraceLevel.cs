namespace Pixel.Persistence.Core.Enums;

/// <summary>
/// Represents different levels in to which trace messages captured during execution of test cases can be categorized
/// </summary>
public enum TraceLevel
{
    /// <summary>
    /// Indicates that trace message is an informational message
    /// </summary>
    Information,

    /// <summary>
    /// Indicates that trace message is a warning message
    /// </summary>
    Warning,

    /// <summary>
    /// /Indicates that trace message is an error message
    /// </summary>
    Error
}
