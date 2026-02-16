namespace JobProcessingApi.Core.Interfaces;
//Interface for the external item processing service

public interface IItemProcessor
{
    //Processes a single item and returns the result

    //<param name="itemData">The data to process</param>
    //<param name="cancellationToken">Cancellation token</param>
    //<returns>Processing result indicating success or failure with description</returns>
    Task<ItemProcessingResult> ProcessAsync(string itemData, CancellationToken cancellationToken = default);
}
//Result of processing a single item

public class ItemProcessingResult
{
    public bool Success { get; set; }
    public string Description { get; set; } = string.Empty;
    public int ProcessingTimeMs { get; set; }
}
