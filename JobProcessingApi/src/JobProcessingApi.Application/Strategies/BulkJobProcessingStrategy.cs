using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JobProcessingApi.Application.Strategies;


//BULK processing strategy: Processes all items in sequence, continues even if one fails
  
public class BulkJobProcessingStrategy : IJobProcessingStrategy
{
    private readonly IItemProcessor _itemProcessor;
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<BulkJobProcessingStrategy> _logger;

    public JobType JobType => JobType.Bulk;

    public BulkJobProcessingStrategy(
        IItemProcessor itemProcessor,
        IJobRepository jobRepository,
        ILogger<BulkJobProcessingStrategy> logger)
    {
        _itemProcessor = itemProcessor ?? throw new ArgumentNullException(nameof(itemProcessor));
        _jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(Job job, IEnumerable<string> items, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting BULK job processing for JobId: {JobId}", job.Id);
        
        var itemsList = items.ToList();
        var itemIndex = 0;

        try
        {
            job.Status = JobStatus.Running;
            job.StartedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);

            foreach (var item in itemsList)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("BULK job processing cancelled for JobId: {JobId}", job.Id);
                    job.Status = JobStatus.PartiallyCompleted;
                    await _jobRepository.UpdateAsync(job, cancellationToken);
                    return;
                }

                await ProcessItemAsync(job, item, itemIndex, cancellationToken);
                itemIndex++;
            }

            // BULK job always completes all items (even with failures)
            job.Status = job.FailedItems > 0 ? JobStatus.PartiallyCompleted : JobStatus.Completed;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);

            _logger.LogInformation(
                "BULK job completed for JobId: {JobId}. Total: {Total}, Processed: {Processed}, Failed: {Failed}",
                job.Id, job.TotalItems, job.ProcessedItems, job.FailedItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in BULK job processing for JobId: {JobId}", job.Id);
            job.Status = JobStatus.Failed;
            job.CompletedAt = DateTime.UtcNow;
            await _jobRepository.UpdateAsync(job, cancellationToken);
            throw;
        }
    }

    private async Task ProcessItemAsync(Job job, string itemData, int itemIndex, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Processing item {Index} for JobId: {JobId}", itemIndex, job.Id);
            
            var result = await _itemProcessor.ProcessAsync(itemData, cancellationToken);
            stopwatch.Stop();

            var log = new JobItemLog
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                ItemIndex = itemIndex,
                ItemData = itemData,
                Status = result.Success ? JobItemStatus.Success : JobItemStatus.Failure,
                Description = result.Description,
                ProcessedAt = DateTime.UtcNow,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };

            await _jobRepository.AddLogAsync(job.Id, log, cancellationToken);

            job.ProcessedItems++;
            if (!result.Success)
            {
                job.FailedItems++;
                _logger.LogWarning(
                    "Item {Index} failed for JobId: {JobId}. Description: {Description}",
                    itemIndex, job.Id, result.Description);
            }

            await _jobRepository.UpdateAsync(job, cancellationToken);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Exception processing item {Index} for JobId: {JobId}", itemIndex, job.Id);

            var log = new JobItemLog
            {
                Id = Guid.NewGuid(),
                JobId = job.Id,
                ItemIndex = itemIndex,
                ItemData = itemData,
                Status = JobItemStatus.Failure,
                Description = $"Exception: {ex.Message}",
                ProcessedAt = DateTime.UtcNow,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };

            await _jobRepository.AddLogAsync(job.Id, log, cancellationToken);
            
            job.ProcessedItems++;
            job.FailedItems++;
            await _jobRepository.UpdateAsync(job, cancellationToken);
        }
    }
}
