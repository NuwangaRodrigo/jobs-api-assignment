using FluentAssertions;
using JobProcessingApi.Application.Strategies;
using JobProcessingApi.Application.Validators;
using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using JobProcessingApi.Infrastructure.Repositories;
using JobProcessingApi.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Xunit;

namespace JobProcessingApi.Tests.Integration;

 
//Integration tests that test the complete flow from controller to repository
  
public class JobProcessingIntegrationTests
{
    [Fact]
    public async Task CompleteJobFlow_BulkJob_ShouldProcessAllItemsSuccessfully()
    {
        // Arrange - Setup the complete stack
        var repository = new InMemoryJobRepository();
        var itemProcessor = new MockItemProcessor(
            new LoggerFactory().CreateLogger<MockItemProcessor>());

        // Create a test job
        var items = new List<string>
        {
            "item-1-SUCCESS",
            "item-2-SUCCESS",
            "item-3-SUCCESS"
        };

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Bulk,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalItems = items.Count,
            ProcessedItems = 0,
            FailedItems = 0
        };

        await repository.CreateAsync(job);

        // Create the strategy with real dependencies
        var bulkStrategy = new BulkJobProcessingStrategy(
            itemProcessor,
            repository,
            new LoggerFactory().CreateLogger<BulkJobProcessingStrategy>());

        // Act - Execute the job
        await bulkStrategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert - Verify the complete flow
        var updatedJob = await repository.GetByIdAsync(job.Id);
        updatedJob.Should().NotBeNull();
        updatedJob!.Status.Should().Be(JobStatus.Completed);
        updatedJob.ProcessedItems.Should().Be(3);
        updatedJob.FailedItems.Should().Be(0);
        updatedJob.CompletedAt.Should().NotBeNull();

        var logs = await repository.GetLogsAsync(job.Id);
        logs.Should().HaveCount(3);
        logs.All(l => l.Status == JobItemStatus.Success).Should().BeTrue();
    }

    [Fact]
    public async Task CompleteJobFlow_BatchJob_ShouldStopOnFirstFailure()
    {
        // Arrange
        var repository = new InMemoryJobRepository();
        var itemProcessor = new MockItemProcessor(
            new LoggerFactory().CreateLogger<MockItemProcessor>());

        var items = new List<string>
        {
            "item-1-SUCCESS",
            "item-2-FAIL",      // This will cause the batch to stop
            "item-3-SUCCESS"
        };

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalItems = items.Count,
            ProcessedItems = 0,
            FailedItems = 0
        };

        await repository.CreateAsync(job);

        var batchStrategy = new BatchJobProcessingStrategy(
            itemProcessor,
            repository,
            new LoggerFactory().CreateLogger<BatchJobProcessingStrategy>());

        // Act
        await batchStrategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert
        var updatedJob = await repository.GetByIdAsync(job.Id);
        updatedJob.Should().NotBeNull();
        updatedJob!.Status.Should().Be(JobStatus.Failed);
        updatedJob.ProcessedItems.Should().Be(2); // Only first two items processed
        updatedJob.FailedItems.Should().Be(1);
        updatedJob.CompletedAt.Should().NotBeNull();

        var logs = await repository.GetLogsAsync(job.Id);
        logs.Should().HaveCount(2); // Only logs for first two items
        logs.ElementAt(0).Status.Should().Be(JobItemStatus.Success);
        logs.ElementAt(1).Status.Should().Be(JobItemStatus.Failure);
    }

    [Fact]
    public async Task CompleteJobFlow_BulkJobWithMixedResults_ShouldProcessAllAndMarkPartiallyCompleted()
    {
        // Arrange
        var repository = new InMemoryJobRepository();
        var itemProcessor = new MockItemProcessor(
            new LoggerFactory().CreateLogger<MockItemProcessor>());

        var items = new List<string>
        {
            "item-1-SUCCESS",
            "item-2-FAIL",
            "item-3-SUCCESS",
            "item-4-FAIL",
            "item-5-SUCCESS"
        };

        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Bulk,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalItems = items.Count,
            ProcessedItems = 0,
            FailedItems = 0
        };

        await repository.CreateAsync(job);

        var bulkStrategy = new BulkJobProcessingStrategy(
            itemProcessor,
            repository,
            new LoggerFactory().CreateLogger<BulkJobProcessingStrategy>());

        // Act
        await bulkStrategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert
        var updatedJob = await repository.GetByIdAsync(job.Id);
        updatedJob.Should().NotBeNull();
        updatedJob!.Status.Should().Be(JobStatus.PartiallyCompleted);
        updatedJob.ProcessedItems.Should().Be(5); // All items processed
        updatedJob.FailedItems.Should().Be(2);
        updatedJob.SuccessfulItems.Should().Be(3);
        updatedJob.ProgressPercentage.Should().Be(100);

        var logs = await repository.GetLogsAsync(job.Id);
        logs.Should().HaveCount(5);
        logs.Count(l => l.Status == JobItemStatus.Success).Should().Be(3);
        logs.Count(l => l.Status == JobItemStatus.Failure).Should().Be(2);
    }

    [Fact]
    public void Validator_WithValidCommand_ShouldPass()
    {
        // Arrange
        var validator = new StartJobCommandValidator();
        var command = new StartJobCommand
        {
            JobType = JobType.Bulk,
            Items = new List<string> { "item1", "item2", "item3" }
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validator_WithEmptyItems_ShouldFail()
    {
        // Arrange
        var validator = new StartJobCommandValidator();
        var command = new StartJobCommand
        {
            JobType = JobType.Bulk,
            Items = new List<string>()
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot be empty"));
    }

    [Fact]
    public void Validator_WithTooManyItems_ShouldFail()
    {
        // Arrange
        var validator = new StartJobCommandValidator();
        var command = new StartJobCommand
        {
            JobType = JobType.Bulk,
            Items = Enumerable.Range(1, 10001).Select(i => $"item{i}").ToList()
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot exceed 10,000"));
    }

    [Fact]
    public void Validator_WithEmptyStringInItems_ShouldFail()
    {
        // Arrange
        var validator = new StartJobCommandValidator();
        var command = new StartJobCommand
        {
            JobType = JobType.Bulk,
            Items = new List<string> { "item1", "", "item3" }
        };

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("cannot contain empty strings"));
    }
}