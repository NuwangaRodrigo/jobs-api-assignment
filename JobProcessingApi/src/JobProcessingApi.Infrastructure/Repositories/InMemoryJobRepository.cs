using JobProcessingApi.Core.Entities;
using JobProcessingApi.Core.Interfaces;
using System.Collections.Concurrent;

namespace JobProcessingApi.Infrastructure.Repositories;


// In-memory implementation of the job repository
// Thread-safe using concurrent collections

public class InMemoryJobRepository : IJobRepository
{
    private readonly ConcurrentDictionary<Guid, Job> _jobs = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentBag<JobItemLog>> _logs = new();

    public Task<Job> CreateAsync(Job job, CancellationToken cancellationToken = default)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        if (!_jobs.TryAdd(job.Id, job))
        {
            throw new InvalidOperationException($"Job with ID {job.Id} already exists.");
        }

        _logs.TryAdd(job.Id, new ConcurrentBag<JobItemLog>());

        return Task.FromResult(job);
    }

    public Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _jobs.TryGetValue(id, out var job);
        return Task.FromResult(job);
    }

    public Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        if (job == null)
        {
            throw new ArgumentNullException(nameof(job));
        }

        if (!_jobs.ContainsKey(job.Id))
        {
            throw new InvalidOperationException($"Job with ID {job.Id} does not exist.");
        }

        _jobs[job.Id] = job;
        return Task.FromResult(job);
    }

    public Task AddLogAsync(Guid jobId, JobItemLog log, CancellationToken cancellationToken = default)
    {
        if (log == null)
        {
            throw new ArgumentNullException(nameof(log));
        }

        if (!_logs.TryGetValue(jobId, out var logs))
        {
            logs = new ConcurrentBag<JobItemLog>();
            _logs.TryAdd(jobId, logs);
        }

        logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<JobItemLog>> GetLogsAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        if (_logs.TryGetValue(jobId, out var logs))
        {
            return Task.FromResult<IEnumerable<JobItemLog>>(logs.OrderBy(l => l.ItemIndex).ToList());
        }

        return Task.FromResult<IEnumerable<JobItemLog>>(Enumerable.Empty<JobItemLog>());
    }

    public Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IEnumerable<Job>>(_jobs.Values.OrderByDescending(j => j.CreatedAt).ToList());
    }
}
