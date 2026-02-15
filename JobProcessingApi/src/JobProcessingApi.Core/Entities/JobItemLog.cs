namespace JobProcessingApi.Core.Entities;

/// <summary>
/// Represents a log entry for a single item processed within a job
/// </summary>
public class JobItemLog
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public int ItemIndex { get; set; }
    public string ItemData { get; set; } = string.Empty;
    public JobItemStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int ProcessingTimeMs { get; set; }
}

/// <summary>
/// Status of an individual item processing
/// </summary>
public enum JobItemStatus
{
    Success,
    Failure
}
