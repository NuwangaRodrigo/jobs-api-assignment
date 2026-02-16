namespace JobProcessingApi.Core.Entities;


// Represents a log entry for a single item processed within a job

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

// Status of an individual item processing

public enum JobItemStatus
{
    Success = 0,
    Failure = 1
}
