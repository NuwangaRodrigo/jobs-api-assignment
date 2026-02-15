namespace JobProcessingApi.Core.Entities;

/// <summary>
/// Represents a job that processes multiple items
/// </summary>
public class Job
{
    public Guid Id { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public List<JobItemLog> Logs { get; set; } = new();

    public int SuccessfulItems => ProcessedItems - FailedItems;
    
    public double ProgressPercentage => TotalItems > 0 
        ? (ProcessedItems * 100.0) / TotalItems 
        : 0;
}

/// <summary>
/// Types of job processing strategies
/// </summary>
public enum JobType
{
    /// <summary>
    /// Processes all items in sequence, continues on failure
    /// </summary>
    Bulk,
    
    /// <summary>
    /// Processes all items in sequence, stops on first failure
    /// </summary>
    Batch
}

/// <summary>
/// Current status of a job
/// </summary>
public enum JobStatus
{
    Pending,
    Running,
    Completed,
    Failed,
    PartiallyCompleted
}
