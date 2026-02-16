using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Application.Strategies;


// Strategy interface for different job processing behaviors

public interface IJobProcessingStrategy
{
    //Gets the job type this strategy handles
      
    JobType JobType { get; }
    
    
    //Executes the processing strategy for a job
      
    Task ExecuteAsync(Job job, IEnumerable<string> items, CancellationToken cancellationToken = default);
}
