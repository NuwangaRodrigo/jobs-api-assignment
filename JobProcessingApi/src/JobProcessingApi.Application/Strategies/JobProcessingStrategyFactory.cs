using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Application.Strategies;

/// <summary>
/// Factory for creating job processing strategies based on job type
/// </summary>
public class JobProcessingStrategyFactory
{
    private readonly IEnumerable<IJobProcessingStrategy> _strategies;

    public JobProcessingStrategyFactory(IEnumerable<IJobProcessingStrategy> strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    /// <summary>
    /// Gets the appropriate strategy for a given job type
    /// </summary>
    public IJobProcessingStrategy GetStrategy(JobType jobType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.JobType == jobType);
        
        if (strategy == null)
        {
            throw new NotSupportedException($"Job type '{jobType}' is not supported.");
        }

        return strategy;
    }
}
