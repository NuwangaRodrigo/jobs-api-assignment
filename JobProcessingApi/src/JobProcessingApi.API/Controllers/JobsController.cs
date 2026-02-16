using FluentValidation;
using JobProcessingApi.Application.Validators;
using JobProcessingApi.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobProcessingApi.API.Controllers;

//Controller for job processing operations
  
[ApiController]
[Route("api/[controller]")]
//[Authorize]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IValidator<StartJobCommand> _validator;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IJobService jobService,
        IValidator<StartJobCommand> validator,
        ILogger<JobsController> logger)
    {
        _jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    
    //Start a new job to process data
      
    //<param name="command">Job configuration including type and items to process</param>
    //<param name="cancellationToken">Cancellation token</param>
    //<returns>The created job ID</returns>
    //<response code="201">Job created successfully</response>
    //<response code="400">Invalid request data</response>
    //<response code="401">Unauthorized</response>
    //<response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(StartJobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StartJob(
        [FromBody] StartJobCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Received request to start {JobType} job with {ItemCount} items",
                command.JobType, command.Items?.Count ?? 0);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Validation failed for start job request");
                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => new ValidationError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList()
                });
            }

            var jobId = await _jobService.StartJobAsync(command.JobType, command.Items, cancellationToken);

            _logger.LogInformation("Job started successfully with JobId: {JobId}", jobId);

            return CreatedAtAction(
                nameof(GetJobStatus),
                new { jobId },
                new StartJobResponse
                {
                    JobId = jobId,
                    Message = "Job created and processing started"
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting job");
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while starting the job",
                Details = ex.Message
            });
        }
    }

    
    //Get the status of a job
      
    //<param name="jobId">The job identifier</param>
    //<param name="cancellationToken">Cancellation token</param>
    //<returns>Current job status including progress</returns>
    //<response code="200">Job status retrieved successfully</response>
    //<response code="404">Job not found</response>
    //<response code="401">Unauthorized</response>
    [HttpGet("{jobId}/status")]
    [ProducesResponseType(typeof(JobStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobStatus(
        [FromRoute] Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving status for JobId: {JobId}", jobId);

            var status = await _jobService.GetJobStatusAsync(jobId, cancellationToken);

            if (status == null)
            {
                _logger.LogWarning("Job not found: {JobId}", jobId);
                return NotFound(new ErrorResponse
                {
                    Message = $"Job with ID {jobId} not found"
                });
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job status for JobId: {JobId}", jobId);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while retrieving job status",
                Details = ex.Message
            });
        }
    }

    
    //Get the processing logs for a job
      
    //<param name="jobId">The job identifier</param>
    //<param name="cancellationToken">Cancellation token</param>
    //<returns>All log entries for the job</returns>
    //<response code="200">Job logs retrieved successfully</response>
    //<response code="404">Job not found</response>
    //<response code="401">Unauthorized</response>
    [HttpGet("{jobId}/logs")]
    [ProducesResponseType(typeof(JobLogsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetJobLogs(
        [FromRoute] Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving logs for JobId: {JobId}", jobId);

            var logs = await _jobService.GetJobLogsAsync(jobId, cancellationToken);

            if (logs == null)
            {
                _logger.LogWarning("Job not found: {JobId}", jobId);
                return NotFound(new ErrorResponse
                {
                    Message = $"Job with ID {jobId} not found"
                });
            }

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving job logs for JobId: {JobId}", jobId);
            return StatusCode(500, new ErrorResponse
            {
                Message = "An error occurred while retrieving job logs",
                Details = ex.Message
            });
        }
    }
}

#region Response Models

public class StartJobResponse
{
    public Guid JobId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ValidationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}

#endregion
