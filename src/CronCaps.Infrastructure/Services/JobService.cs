using AutoMapper;
using CronCaps.Application.DTOs;
using CronCaps.Application.UseCases;
using CronCaps.Domain.Entities;
using CronCaps.Domain.Enums;
using CronCaps.Domain.Exceptions;
using CronCaps.Domain.Interfaces;

namespace CronCaps.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public JobService(
        IJobRepository jobRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _jobRepository = jobRepository;
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<JobDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        return _mapper.Map<JobDto>(job);
    }

    public async Task<IEnumerable<JobDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<IEnumerable<JobDto>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<IEnumerable<JobDto>> GetByTeamAsync(Guid teamId, CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetByTeamIdAsync(teamId, cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<IEnumerable<JobDto>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetByCategoryIdAsync(categoryId, cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<JobDto> CreateAsync(CreateJobDto createJobDto, Guid createdById, CancellationToken cancellationToken = default)
    {
        // Verify creator exists
        var creator = await _userRepository.GetByIdAsync(createdById, cancellationToken);
        if (creator == null)
        {
            throw new ArgumentException("Creator not found", nameof(createdById));
        }

        var job = _mapper.Map<Job>(createJobDto, opts => opts.Items["CreatedById"] = createdById);

        var createdJob = await _jobRepository.AddAsync(job, cancellationToken);

        // Reload with navigation properties
        var jobWithNavigation = await _jobRepository.GetByIdAsync(createdJob.Id, cancellationToken);
        return _mapper.Map<JobDto>(jobWithNavigation);
    }

    public async Task<JobDto> UpdateAsync(Guid id, UpdateJobDto updateJobDto, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        job.UpdateDetails(
            updateJobDto.Name,
            updateJobDto.Description,
            updateJobDto.CronExpression,
            updateJobDto.Tags);

        if (updateJobDto.Configuration != null)
        {
            var configuration = _mapper.Map<Domain.ValueObjects.JobConfiguration>(updateJobDto.Configuration);
            job.UpdateConfiguration(configuration);
        }

        var updatedJob = await _jobRepository.UpdateAsync(job, cancellationToken);
        return _mapper.Map<JobDto>(updatedJob);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        await _jobRepository.DeleteAsync(job, cancellationToken);
    }

    public async Task<JobDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        job.Activate();
        var updatedJob = await _jobRepository.UpdateAsync(job, cancellationToken);
        return _mapper.Map<JobDto>(updatedJob);
    }

    public async Task<JobDto> PauseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        job.Pause();
        var updatedJob = await _jobRepository.UpdateAsync(job, cancellationToken);
        return _mapper.Map<JobDto>(updatedJob);
    }

    public async Task<JobDto> ResumeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        job.Resume();
        var updatedJob = await _jobRepository.UpdateAsync(job, cancellationToken);
        return _mapper.Map<JobDto>(updatedJob);
    }

    public async Task<JobExecutionDto> ExecuteManuallyAsync(Guid id, Guid triggeredById, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        var user = await _userRepository.GetByIdAsync(triggeredById, cancellationToken);
        var triggeredBy = user?.Name ?? "Unknown";

        // Create job execution record
        var execution = JobExecution.Create(id, triggeredBy);

        // For now, this will just create the execution record
        // The actual execution logic would be handled by the background service
        execution.Start();

        // Simulate success for now
        execution.Complete("Job executed manually");

        return _mapper.Map<JobExecutionDto>(execution);
    }

    public async Task<JobExecutionHistoryDto> GetExecutionHistoryAsync(Guid id, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);
        if (job == null)
        {
            throw new JobNotFoundException($"Job with ID {id} not found");
        }

        var executions = job.Executions
            .OrderByDescending(e => e.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalCount = job.Executions.Count;

        return new JobExecutionHistoryDto(
            _mapper.Map<List<JobExecutionDto>>(executions),
            totalCount,
            pageNumber,
            pageSize,
            pageNumber * pageSize < totalCount,
            pageNumber > 1
        );
    }

    public async Task<IEnumerable<JobDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.SearchAsync(query, cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }

    public async Task<IEnumerable<JobDto>> GetJobsDueForExecutionAsync(CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetJobsDueForExecutionAsync(cancellationToken);
        return _mapper.Map<IEnumerable<JobDto>>(jobs);
    }
}