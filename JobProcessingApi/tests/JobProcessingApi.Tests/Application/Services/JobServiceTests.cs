using FluentAssertions;
using JobProcessingApi.Application.Services;
using JobProcessingApi.Application.Strategies;
using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobProcessingApi.Tests.Application.Services;

public class JobServiceTests
{
    private readonly Mock<IJobRepository> _jobRepositoryMock;
    private readonly Mock<JobProcessingStrategyFactory> _strategyFactoryMock;
    private readonly Mock<ILogger<JobService>> _loggerMock;
    private readonly JobService _jobService;

    public JobServiceTests()
    {
        _jobRepositoryMock = new Mock<IJobRepository>();
        
        // Create a mock strategy factory
        var strategies = new List<IJobProcessingStrategy>();
        _strategyFactoryMock = new Mock<JobProcessingStrategyFactory>(strategies);
        
        _loggerMock = new Mock<ILogger<JobService>>();
        _jobService = new JobService(_jobRepositoryMock.Object, _strategyFactoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task StartJobAsync_WithValidItems_ShouldCreateJobAndReturnId()
    {
        // Arrange
        var items = new List<string> { "item1", "item2", "item3" };
        var jobType = JobType.Bulk;

        Job? capturedJob = null;
        _jobRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job j, CancellationToken _) =>
            {
                capturedJob = j;
                return j;
            });

        // Act
        var jobId = await _jobService.StartJobAsync(jobType, items, CancellationToken.None);

        // Assert
        jobId.Should().NotBeEmpty();
        capturedJob.Should().NotBeNull();
        capturedJob!.Type.Should().Be(JobType.Bulk);
        capturedJob.TotalItems.Should().Be(3);
        capturedJob.Status.Should().Be(JobStatus.Pending);
        capturedJob.ProcessedItems.Should().Be(0);
        capturedJob.FailedItems.Should().Be(0);

        _jobRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Job>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StartJobAsync_WithEmptyItems_ShouldThrowArgumentException()
    {
        // Arrange
        var items = new List<string>();

        // Act
        Func<Task> act = async () => await _jobService.StartJobAsync(JobType.Bulk, items, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Items collection cannot be null or empty.*");
    }

    [Fact]
    public async Task StartJobAsync_WithNullItems_ShouldThrowArgumentException()
    {
        // Act
        Func<Task> act = async () => await _jobService.StartJobAsync(JobType.Bulk, null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Items collection cannot be null or empty.*");
    }

    [Fact]
    public async Task GetJobStatusAsync_WithExistingJob_ShouldReturnStatus()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new Job
        {
            Id = jobId,
            Type = JobType.Bulk,
            Status = JobStatus.Running,
            TotalItems = 10,
            ProcessedItems = 5,
            FailedItems = 1,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            StartedAt = DateTime.UtcNow.AddMinutes(-4)
        };

        _jobRepositoryMock
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        // Act
        var status = await _jobService.GetJobStatusAsync(jobId, CancellationToken.None);

        // Assert
        status.Should().NotBeNull();
        status!.JobId.Should().Be(jobId);
        status.Type.Should().Be(JobType.Bulk);
        status.Status.Should().Be(JobStatus.Running);
        status.TotalItems.Should().Be(10);
        status.ProcessedItems.Should().Be(5);
        status.FailedItems.Should().Be(1);
        status.SuccessfulItems.Should().Be(4);
        status.ProgressPercentage.Should().Be(50);
    }

    [Fact]
    public async Task GetJobStatusAsync_WithNonExistentJob_ShouldReturnNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _jobRepositoryMock
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        // Act
        var status = await _jobService.GetJobStatusAsync(jobId, CancellationToken.None);

        // Assert
        status.Should().BeNull();
    }

    [Fact]
    public async Task GetJobLogsAsync_WithExistingJob_ShouldReturnLogs()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job = new Job { Id = jobId, Type = JobType.Bulk, Status = JobStatus.Completed };

        var logs = new List<JobItemLog>
        {
            new JobItemLog { ItemIndex = 0, ItemData = "item1", Status = JobItemStatus.Success, Description = "Success" },
            new JobItemLog { ItemIndex = 1, ItemData = "item2", Status = JobItemStatus.Failure, Description = "Failed" }
        };

        _jobRepositoryMock
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(job);

        _jobRepositoryMock
            .Setup(x => x.GetLogsAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _jobService.GetJobLogsAsync(jobId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.JobId.Should().Be(jobId);
        result.Logs.Should().HaveCount(2);
        result.Logs[0].ItemIndex.Should().Be(0);
        result.Logs[0].Status.Should().Be(JobItemStatus.Success);
        result.Logs[1].ItemIndex.Should().Be(1);
        result.Logs[1].Status.Should().Be(JobItemStatus.Failure);
    }

    [Fact]
    public async Task GetJobLogsAsync_WithNonExistentJob_ShouldReturnNull()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _jobRepositoryMock
            .Setup(x => x.GetByIdAsync(jobId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Job?)null);

        // Act
        var result = await _jobService.GetJobLogsAsync(jobId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}
