using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Core.Interfaces;

/// <summary>
/// Service interface for job operations
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Starts a new job with the provided data items
    /// </summary>
    Task<Guid> StartJobAsync(JobType jobType, IEnumerable<string> items, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current status of a job
    /// </summary>
    Task<JobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all logs for a specific job
    /// </summary>
    Task<JobLogsDto?> GetJobLogsAsync(Guid jobId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Data transfer object for job status
/// </summary>
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

/// <summary>
/// Data transfer object for job logs
/// </summary>
public class JobLogsDto
{
    public Guid JobId { get; set; }
    public List<JobItemLogDto> Logs { get; set; } = new();
}

/// <summary>
/// Data transfer object for individual log entry
/// </summary>
public class JobItemLogDto
{
    public int ItemIndex { get; set; }
    public string ItemData { get; set; } = string.Empty;
    public JobItemStatus Status { get; set; }
    public string? Description { get; set; }
    public DateTime ProcessedAt { get; set; }
    public int ProcessingTimeMs { get; set; }
}
