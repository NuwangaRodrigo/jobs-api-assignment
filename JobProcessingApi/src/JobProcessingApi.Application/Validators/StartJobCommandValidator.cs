using FluentValidation;
using JobProcessingApi.Core.Entities;

namespace JobProcessingApi.Application.Validators;

/// <summary>
/// Request model for starting a job
/// </summary>
public class StartJobCommand
{
    public JobType JobType { get; set; }
    public List<string> Items { get; set; } = new();
}

/// <summary>
/// Validator for StartJobCommand
/// </summary>
public class StartJobCommandValidator : AbstractValidator<StartJobCommand>
{
    public StartJobCommandValidator()
    {
        RuleFor(x => x.JobType)
            .IsInEnum()
            .WithMessage("Invalid job type. Must be 'Bulk' or 'Batch'.");

        RuleFor(x => x.Items)
            .NotNull()
            .WithMessage("Items collection is required.")
            .NotEmpty()
            .WithMessage("Items collection cannot be empty.")
            .Must(items => items.Count <= 10000)
            .WithMessage("Items collection cannot exceed 10,000 items.");

        RuleForEach(x => x.Items)
            .NotEmpty()
            .WithMessage("Items cannot contain empty strings.")
            .MaximumLength(1000)
            .WithMessage("Item data cannot exceed 1,000 characters.");
    }
}
