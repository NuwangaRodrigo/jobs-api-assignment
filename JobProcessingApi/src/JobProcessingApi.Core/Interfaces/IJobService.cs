using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Core.Interfaces;

// Service interface for job operations
public interface IJobService
{
    // Starts a new job with the provided data items
    Task<Guid> StartJobAsync(JobType jobType, IEnumerable<string> items, CancellationToken cancellationToken = default);
    

    // Gets the current status of a job

    Task<JobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    // Gets all logs for a specific job

    Task<JobLogsDto?> GetJobLogsAsync(Guid jobId, CancellationToken cancellationToken = default);
}

// Data transfer object for job status

public class JobStatusDto
{
    public Guid JobId { get; set; }
    public JobType Type { get; set; }
    public JobStatus Status { get; set; }
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public int FailedItems { get; set; }
    public int SuccessfulItems { get; set; }
    public double ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}


// Data transfer object for job logs

public class JobLogsDto
{
    public Guid JobId { get; set; }
    public List<JobItemLogDto> Logs { get; set; } = new();
}

// Data transfer object for individual log entry

public class JobItemLogDto
{
    public int ItemIndex { get; set; }
    public string ItemData { get; set; } = string.Empty;
    public JobItemStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int ProcessingTimeMs { get; set; }
}
