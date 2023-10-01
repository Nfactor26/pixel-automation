namespace Pixel.Persistence.Core.Models;

/// <summary>
/// RetentionPolicy helps to configure what revisions of project and prefab data files stored 
/// in database should be kept around
/// </summary>
public class RetentionPolicy
{
    /// <summary>
    /// Maximum number of allowed file revisions.
    /// Revisions above allowed numbers will be purged.
    /// </summary>
    public int MaxNumberOfRevisions { get; set; }

    /// <summary>
    /// Maximum Age of allowed file revisions in days.
    /// Revisions older then max age will be purged.
    /// </summary>
    public int MaxAgeOfRevisions { get; set; }

    /// <summary>
    /// Number of days to keep TestSession and all it's related data ( test results + trace images).
    /// TestSession will be deleted after TestSessionRetentionPeriod days have passed since it was captured.
    /// </summary>
    public int TestSessionRetentionPeriod { get; set; }
}
