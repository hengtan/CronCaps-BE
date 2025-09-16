using CronCaps.Domain.Entities;
using CronCaps.Domain.Enums;
using CronCaps.Domain.Interfaces;
using CronCaps.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CronCaps.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly CronCapsDbContext _context;

    public JobRepository(CronCapsDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status != JobStatus.Deleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.CreatedById == userId && j.Status != JobStatus.Deleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByTeamIdAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.TeamId == teamId && j.Status != JobStatus.Deleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.CategoryId == categoryId && j.Status != JobStatus.Deleted)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetJobsDueForExecutionAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status == JobStatus.Active &&
                       j.NextRunAt.HasValue &&
                       j.NextRunAt <= now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetJobsDueForExecutionAsync(DateTime asOfTime, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status == JobStatus.Active &&
                       j.NextRunAt.HasValue &&
                       j.NextRunAt <= asOfTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetByStatusAsync(JobStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
    {
        return await GetByStatusAsync(JobStatus.Active, cancellationToken);
    }

    public async Task<Job?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .FirstOrDefaultAsync(j => j.Name == name && j.Status != JobStatus.Deleted, cancellationToken);
    }

    public async Task<IEnumerable<Job>> SearchJobsAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await SearchAsync(searchTerm, cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetJobsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await GetByCategoryIdAsync(categoryId, cancellationToken);
    }

    public async Task<IEnumerable<Job>> GetJobsByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status != JobStatus.Deleted &&
                       j.Tags != null &&
                       j.Tags.ToLower().Contains(tag.ToLower()))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeJobId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Jobs.Where(j => j.Name == name && j.Status != JobStatus.Deleted);

        if (excludeJobId.HasValue)
        {
            query = query.Where(j => j.Id != excludeJobId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Job>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var searchTerm = query.ToLower();
        return await _context.Jobs
            .Include(j => j.CreatedBy)
            .Include(j => j.Team)
            .Include(j => j.Category)
            .Where(j => j.Status != JobStatus.Deleted &&
                       (j.Name.ToLower().Contains(searchTerm) ||
                        (j.Description != null && j.Description.ToLower().Contains(searchTerm)) ||
                        (j.Tags != null && j.Tags.ToLower().Contains(searchTerm))))
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Job> AddAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task<Job> UpdateAsync(Job job, CancellationToken cancellationToken = default)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task DeleteAsync(Job job, CancellationToken cancellationToken = default)
    {
        job.Delete();
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Jobs.AnyAsync(j => j.Id == id && j.Status != JobStatus.Deleted, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Jobs.CountAsync(j => j.Status != JobStatus.Deleted, cancellationToken);
    }
}