using CronCaps.Application.DTOs;
using CronCaps.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace CronCaps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetJobs(
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? teamId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<JobDto> jobs;

            if (!string.IsNullOrWhiteSpace(search))
            {
                jobs = await _jobService.SearchAsync(search, cancellationToken);
            }
            else if (userId.HasValue)
            {
                jobs = await _jobService.GetByUserAsync(userId.Value, cancellationToken);
            }
            else if (teamId.HasValue)
            {
                jobs = await _jobService.GetByTeamAsync(teamId.Value, cancellationToken);
            }
            else if (categoryId.HasValue)
            {
                jobs = await _jobService.GetByCategoryAsync(categoryId.Value, cancellationToken);
            }
            else
            {
                jobs = await _jobService.GetAllAsync(cancellationToken);
            }

            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs");
            return StatusCode(500, "An error occurred while retrieving jobs");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDto>> GetJob(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.GetByIdAsync(id, cancellationToken);
            return Ok(job);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job {JobId}", id);
            return StatusCode(500, "An error occurred while retrieving the job");
        }
    }

    [HttpPost]
    public async Task<ActionResult<JobDto>> CreateJob(
        CreateJobDto createJobDto,
        [FromHeader(Name = "X-User-Id")] Guid createdById,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.CreateAsync(createJobDto, createdById, cancellationToken);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating job");
            return StatusCode(500, "An error occurred while creating the job");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobDto>> UpdateJob(
        Guid id,
        UpdateJobDto updateJobDto,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.UpdateAsync(id, updateJobDto, cancellationToken);
            return Ok(job);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job {JobId}", id);
            return StatusCode(500, "An error occurred while updating the job");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteJob(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _jobService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting job {JobId}", id);
            return StatusCode(500, "An error occurred while deleting the job");
        }
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<JobDto>> ActivateJob(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.ActivateAsync(id, cancellationToken);
            return Ok(job);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating job {JobId}", id);
            return StatusCode(500, "An error occurred while activating the job");
        }
    }

    [HttpPost("{id:guid}/pause")]
    public async Task<ActionResult<JobDto>> PauseJob(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.PauseAsync(id, cancellationToken);
            return Ok(job);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing job {JobId}", id);
            return StatusCode(500, "An error occurred while pausing the job");
        }
    }

    [HttpPost("{id:guid}/resume")]
    public async Task<ActionResult<JobDto>> ResumeJob(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var job = await _jobService.ResumeAsync(id, cancellationToken);
            return Ok(job);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming job {JobId}", id);
            return StatusCode(500, "An error occurred while resuming the job");
        }
    }

    [HttpPost("{id:guid}/execute")]
    public async Task<ActionResult<JobExecutionDto>> ExecuteJob(
        Guid id,
        [FromHeader(Name = "X-User-Id")] Guid triggeredById,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var execution = await _jobService.ExecuteManuallyAsync(id, triggeredById, cancellationToken);
            return Ok(execution);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing job {JobId}", id);
            return StatusCode(500, "An error occurred while executing the job");
        }
    }

    [HttpGet("{id:guid}/executions")]
    public async Task<ActionResult<JobExecutionHistoryDto>> GetJobExecutions(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var history = await _jobService.GetExecutionHistoryAsync(id, pageNumber, pageSize, cancellationToken);
            return Ok(history);
        }
        catch (ArgumentException)
        {
            return NotFound($"Job with ID {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution history for job {JobId}", id);
            return StatusCode(500, "An error occurred while retrieving execution history");
        }
    }

    [HttpGet("due")]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetJobsDueForExecution(CancellationToken cancellationToken = default)
    {
        try
        {
            var jobs = await _jobService.GetJobsDueForExecutionAsync(cancellationToken);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs due for execution");
            return StatusCode(500, "An error occurred while retrieving jobs due for execution");
        }
    }
}