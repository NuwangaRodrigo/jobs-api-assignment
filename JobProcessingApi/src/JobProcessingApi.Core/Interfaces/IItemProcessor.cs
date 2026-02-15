namespace JobProcessingApi.Core.Interfaces;

/// <summary>
/// Interface for the external item processing service
/// </summary>
public interface IItemProcessor
{
    /// <summary>
    /// Processes a single item and returns the result
    /// </summary>
    /// <param name="itemData">The data to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Processing result indicating success or failure with description</returns>
    Task<ItemProcessingResult> ProcessAsync(string itemData, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of processing a single item
/// </summary>
public class ItemProcessingResult
{
    public bool Success { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ProcessingTimeMs { get; set; }
}
