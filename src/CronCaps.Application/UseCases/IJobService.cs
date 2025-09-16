using CronCaps.Application.DTOs;

namespace CronCaps.Application.UseCases;

public interface IJobService
{
    Task<JobDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> GetByTeamAsync(Guid teamId, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<JobDto> CreateAsync(CreateJobDto createJobDto, Guid createdById, CancellationToken cancellationToken = default);
    Task<JobDto> UpdateAsync(Guid id, UpdateJobDto updateJobDto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobDto> PauseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobDto> ResumeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JobExecutionDto> ExecuteManuallyAsync(Guid id, Guid triggeredById, CancellationToken cancellationToken = default);
    Task<JobExecutionHistoryDto> GetExecutionHistoryAsync(Guid id, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    Task<IEnumerable<JobDto>> GetJobsDueForExecutionAsync(CancellationToken cancellationToken = default);
}