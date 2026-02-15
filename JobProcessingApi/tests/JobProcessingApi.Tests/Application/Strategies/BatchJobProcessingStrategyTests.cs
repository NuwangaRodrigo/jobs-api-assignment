using FluentAssertions;
using JobProcessingApi.Application.Strategies;
using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobProcessingApi.Tests.Application.Strategies;

public class BatchJobProcessingStrategyTests
{
    private readonly Mock<IItemProcessor> _itemProcessorMock;
    private readonly Mock<IJobRepository> _jobRepositoryMock;
    private readonly Mock<ILogger<BatchJobProcessingStrategy>> _loggerMock;
    private readonly BatchJobProcessingStrategy _strategy;

    public BatchJobProcessingStrategyTests()
    {
        _itemProcessorMock = new Mock<IItemProcessor>();
        _jobRepositoryMock = new Mock<IJobRepository>();
        _loggerMock = new Mock<ILogger<BatchJobProcessingStrategy>>();
        _strategy = new BatchJobProcessingStrategy(_itemProcessorMock.Object, _jobRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void JobType_ShouldReturnBatch()
    {
        // Act
        var jobType = _strategy.JobType;

        // Assert
        jobType.Should().Be(JobType.Batch);
    }

    [Fact]
    public async Task ExecuteAsync_WithAllSuccessfulItems_ShouldCompleteSuccessfully()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            Status = JobStatus.Pending,
            TotalItems = 3,
            ProcessedItems = 0,
            FailedItems = 0
        };

        var items = new List<string> { "item1", "item2", "item3" };

        _itemProcessorMock
            .Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemProcessingResult { Success = true, Description = "Success", ProcessingTimeMs = 100 });

        _jobRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job j, CancellationToken _) => j);

        _jobRepositoryMock
            .Setup(x => x.AddLogAsync(It.IsAny<Guid>(), It.IsAny<JobItemLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _strategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert
        job.Status.Should().Be(JobStatus.Completed);
        job.ProcessedItems.Should().Be(3);
        job.FailedItems.Should().Be(0);
        job.CompletedAt.Should().NotBeNull();

        _itemProcessorMock.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ExecuteAsync_WithFirstItemFailure_ShouldStopImmediately()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            Status = JobStatus.Pending,
            TotalItems = 3,
            ProcessedItems = 0,
            FailedItems = 0
        };

        var items = new List<string> { "item1", "item2", "item3" };

        _itemProcessorMock
            .Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ItemProcessingResult { Success = false, Description = "Failed", ProcessingTimeMs = 100 });

        _jobRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job j, CancellationToken _) => j);

        _jobRepositoryMock
            .Setup(x => x.AddLogAsync(It.IsAny<Guid>(), It.IsAny<JobItemLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _strategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert
        job.Status.Should().Be(JobStatus.Failed);
        job.ProcessedItems.Should().Be(1);
        job.FailedItems.Should().Be(1);
        job.CompletedAt.Should().NotBeNull();

        // Should only process the first item
        _itemProcessorMock.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithSecondItemFailure_ShouldStopAfterSecondItem()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            Status = JobStatus.Pending,
            TotalItems = 3,
            ProcessedItems = 0,
            FailedItems = 0
        };

        var items = new List<string> { "item1", "item2", "item3" };

        var callCount = 0;
        _itemProcessorMock
            .Setup(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return new ItemProcessingResult
                {
                    Success = callCount != 2, // Fail the second item
                    Description = callCount == 2 ? "Failed" : "Success",
                    ProcessingTimeMs = 100
                };
            });

        _jobRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job j, CancellationToken _) => j);

        _jobRepositoryMock
            .Setup(x => x.AddLogAsync(It.IsAny<Guid>(), It.IsAny<JobItemLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _strategy.ExecuteAsync(job, items, CancellationToken.None);

        // Assert
        job.Status.Should().Be(JobStatus.Failed);
        job.ProcessedItems.Should().Be(2);
        job.FailedItems.Should().Be(1);
        job.CompletedAt.Should().NotBeNull();

        // Should stop after processing first two items
        _itemProcessorMock.Verify(x => x.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
