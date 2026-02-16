using JobProcessingApi.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JobProcessingApi.Infrastructure.Services;

// Mock implementation of the external item processor
// Simulates processing with configurable delay and failure rate

public class MockItemProcessor : IItemProcessor
{
    private readonly ILogger<MockItemProcessor> _logger;
    private readonly Random _random = new();
    private const int AverageProcessingTimeMs = 500;
    private const double FailureRate = 0.1; // 10% failure rate for testing

    public MockItemProcessor(ILogger<MockItemProcessor> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ItemProcessingResult> ProcessAsync(string itemData, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Simulate variable processing time (400-600ms average)
            var processingTime = AverageProcessingTimeMs + _random.Next(-100, 100);
            await Task.Delay(processingTime, cancellationToken);

            // Simulate occasional failures for realistic testing
            var shouldFail = _random.NextDouble() < FailureRate;

            // Special handling for specific test patterns
            if (itemData.Contains("FAIL", StringComparison.OrdinalIgnoreCase))
            {
                shouldFail = true;
            }
            else if (itemData.Contains("SUCCESS", StringComparison.OrdinalIgnoreCase))
            {
                shouldFail = false;
            }

            stopwatch.Stop();

            if (shouldFail)
            {
                _logger.LogDebug("Item processing failed: {ItemData}", itemData);
                return new ItemProcessingResult
                {
                    Success = false,
                    Description = $"Processing failed for item: {itemData}",
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
                };
            }

            _logger.LogDebug("Item processing succeeded: {ItemData}", itemData);
            return new ItemProcessingResult
            {
                Success = true,
                Description = $"Successfully processed item: {itemData}",
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            _logger.LogWarning("Item processing cancelled: {ItemData}", itemData);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Unexpected error processing item: {ItemData}", itemData);
            return new ItemProcessingResult
            {
                Success = false,
                Description = $"Unexpected error: {ex.Message}",
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }
}
