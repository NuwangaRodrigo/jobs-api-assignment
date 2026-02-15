using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Application.Strategies;

/// <summary>
/// Strategy interface for different job processing behaviors
/// </summary>
public interface IJobProcessingStrategy
{
    /// <summary>
    /// Gets the job type this strategy handles
    /// </summary>
    JobType JobType { get; }
    
    /// <summary>
    /// Executes the processing strategy for a job
    /// </summary>
    Task ExecuteAsync(Job job, IEnumerable<string> items, CancellationToken cancellationToken = default);
}
