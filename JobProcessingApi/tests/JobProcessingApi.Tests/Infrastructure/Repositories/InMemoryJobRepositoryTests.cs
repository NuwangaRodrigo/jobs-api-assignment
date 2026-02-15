using FluentAssertions;
using JobProcessingApi.Core.Entities;
using JobProcessingApi.Infrastructure.Repositories;
using Xunit;

namespace JobProcessingApi.Tests.Infrastructure.Repositories;

public class InMemoryJobRepositoryTests
{
    private readonly InMemoryJobRepository _repository;

    public InMemoryJobRepositoryTests()
    {
        _repository = new InMemoryJobRepository();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateJob()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Bulk,
            Status = JobStatus.Pending,
            TotalItems = 5
        };

        // Act
        var result = await _repository.CreateAsync(job);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(job.Id);
        result.Type.Should().Be(job.Type);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateId_ShouldThrowException()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var job1 = new Job { Id = jobId, Type = JobType.Bulk };
        var job2 = new Job { Id = jobId, Type = JobType.Batch };

        await _repository.CreateAsync(job1);

        // Act
        Func<Task> act = async () => await _repository.CreateAsync(job2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Job with ID {jobId} already exists.");
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingJob_ShouldReturnJob()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            Status = JobStatus.Running
        };
        await _repository.CreateAsync(job);

        // Act
        var result = await _repository.GetByIdAsync(job.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(job.Id);
        result.Type.Should().Be(job.Type);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentJob_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateJob()
    {
        // Arrange
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Bulk,
            Status = JobStatus.Pending,
            ProcessedItems = 0
        };
        await _repository.CreateAsync(job);

        // Act
        job.Status = JobStatus.Running;
        job.ProcessedItems = 5;
        await _repository.UpdateAsync(job);

        var updated = await _repository.GetByIdAsync(job.Id);

        // Assert
        updated.Should().NotBeNull();
        updated!.Status.Should().Be(JobStatus.Running);
        updated.ProcessedItems.Should().Be(5);
    }

    [Fact]
    public async Task AddLogAsync_ShouldAddLog()
    {
        // Arrange
        var job = new Job { Id = Guid.NewGuid(), Type = JobType.Bulk };
        await _repository.CreateAsync(job);

        var log = new JobItemLog
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            ItemIndex = 0,
            ItemData = "test",
            Status = JobItemStatus.Success,
            Description = "Test log"
        };

        // Act
        await _repository.AddLogAsync(job.Id, log);
        var logs = await _repository.GetLogsAsync(job.Id);

        // Assert
        logs.Should().HaveCount(1);
        logs.First().ItemData.Should().Be("test");
        logs.First().Status.Should().Be(JobItemStatus.Success);
    }

    [Fact]
    public async Task GetLogsAsync_ShouldReturnLogsInOrder()
    {
        // Arrange
        var job = new Job { Id = Guid.NewGuid(), Type = JobType.Bulk };
        await _repository.CreateAsync(job);

        var log1 = new JobItemLog { Id = Guid.NewGuid(), JobId = job.Id, ItemIndex = 2 };
        var log2 = new JobItemLog { Id = Guid.NewGuid(), JobId = job.Id, ItemIndex = 0 };
        var log3 = new JobItemLog { Id = Guid.NewGuid(), JobId = job.Id, ItemIndex = 1 };

        await _repository.AddLogAsync(job.Id, log1);
        await _repository.AddLogAsync(job.Id, log2);
        await _repository.AddLogAsync(job.Id, log3);

        // Act
        var logs = await _repository.GetLogsAsync(job.Id);

        // Assert
        logs.Should().HaveCount(3);
        logs.ElementAt(0).ItemIndex.Should().Be(0);
        logs.ElementAt(1).ItemIndex.Should().Be(1);
        logs.ElementAt(2).ItemIndex.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllJobs()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var job1 = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Bulk,
            CreatedAt = now.AddSeconds(-1)  // Created 1 second earlier
        };
        var job2 = new Job
        {
            Id = Guid.NewGuid(),
            Type = JobType.Batch,
            CreatedAt = now  // Created more recently
        };

        await _repository.CreateAsync(job1);
        await _repository.CreateAsync(job2);

        // Act
        var jobs = await _repository.GetAllAsync();

        // Assert
        jobs.Should().HaveCount(2);
        // Should be ordered by CreatedAt descending (most recent first)
        jobs.First().Id.Should().Be(job2.Id);
    }
}