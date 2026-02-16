using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Core.Interfaces;


// Repository interface for job data access

public interface IJobRepository
{
    
    // Creates a new job in the repository
    Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default);
    
 
    // Retrieves a job by its ID
    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
 
    // Updates an existing job
    Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default);
    
   
    // Adds a log entry to a job
    Task AddLogAsync(Guid jobId, JobItemLog log, CancellationToken cancellationToken = default);
    
  
    // Retrieves all logs for a specific job
    Task<IEnumerable<JobItemLog>> GetLogsAsync(Guid jobId, CancellationToken cancellationToken = default);
    
  
    // Retrieves all jobs (for admin/monitoring purposes)
    Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default);
}
