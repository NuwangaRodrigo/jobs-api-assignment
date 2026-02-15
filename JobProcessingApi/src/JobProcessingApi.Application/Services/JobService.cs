using JobProcessingApi.Application.Strategies;
using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobProcessingApi.Application.Services;

/// <summary>
/// Service implementation for job operations
/// </summary>
public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly JobProcessingStrategyFactory _strategyFactory;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository jobRepository,
        JobProcessingStrategyFactory strategyFactory,
        ILogger<JobService> logger)
    {
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        _strategyFactory = strategyFactory ?? throw new ArgumentNullException(nameof(strategyFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Guid> StartJobAsync(JobType jobType, IEnumerable<string> items, CancellationToken cancellationToken = default)
    {
        if (items == null || !items.Any())
        {
            throw new ArgumentException("Items collection cannot be null or empty.", nameof(items));
        }

        var itemsList = items.ToList();
        
        _logger.LogInformation("Starting new {JobType} job with {ItemCount} items", jobType, itemsList.Count);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = jobType,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalItems = itemsList.Count,
            ProcessedItems = 0,
            FailedItems = 0
        };

        await _jobRepository.CreateAsync(job, cancellationToken);

        _logger.LogInformation("Job created with JobId: {JobId}", job.Id);

        // Start processing asynchronously without awaiting
        _ = Task.Run(async () =>
        {
            try
            {
                var strategy = _strategyFactory.GetStrategy(jobType);
                await strategy.ExecuteAsync(job, itemsList, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background job processing failed for JobId: {JobId}", job.Id);
            }
        }, CancellationToken.None);

        return job.Id;
    }

    public async Task<JobStatusDto?> GetJobStatusAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving status for JobId: {JobId}", jobId);

        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        
        if (job == null)
        {
            _logger.LogWarning("Job not found: {JobId}", jobId);
            return null;
        }

        return new JobStatusDto
        {
            JobId = job.Id,
            Type = job.Type,
            Status = job.Status,
            TotalItems = job.TotalItems,
            ProcessedItems = job.ProcessedItems,
            FailedItems = job.FailedItems,
            SuccessfulItems = job.SuccessfulItems,
            ProgressPercentage = job.ProgressPercentage,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt
        };
    }

    public async Task<JobLogsDto?> GetJobLogsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving logs for JobId: {JobId}", jobId);

        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        
        if (job == null)
        {
            _logger.LogWarning("Job not found: {JobId}", jobId);
            return null;
        }

        var logs = await _jobRepository.GetLogsAsync(jobId, cancellationToken);

        return new JobLogsDto
        {
            JobId = jobId,
            Logs = logs.Select(l => new JobItemLogDto
            {
                ItemIndex = l.ItemIndex,
                ItemData = l.ItemData,
                Status = l.Status,
                Description = l.Description,
                ProcessedAt = l.ProcessedAt,
                ProcessingTimeMs = l.ProcessingTimeMs
            }).ToList()
        };
    }
}
