using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Core.Interfaces;

/// <summary>
/// Repository interface for job data access
/// </summary>
public interface IJobRepository
{
    /// <summary>
    /// Creates a new job in the repository
    /// </summary>
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves a job by its ID
    /// </summary>
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates an existing job
    /// </summary>
    Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Adds a log entry to a job
    /// </summary>
    Task AddLogAsync(Guid jobId, JobItemLog log, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all logs for a specific job
    /// </summary>
    Task<IEnumerable<JobItemLog>> GetLogsAsync(Guid jobId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Retrieves all jobs (for admin/monitoring purposes)
    /// </summary>
    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default);
}
