namespace JobProcessingApi.Core.Entities;

// Represents a job that processes multiple items

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

// Types of job processing strategies

public enum JobType
{
    Bulk = 0, // Processes all items in sequence, continues on failure
    Batch = 1 // Processes all items in sequence, stops on first failure
}


// Current status of a job

public enum JobStatus
{
    Pending = 0,
    Running = 1,
    Completed =2,
    Failed =3,
    PartiallyCompleted =4
}
