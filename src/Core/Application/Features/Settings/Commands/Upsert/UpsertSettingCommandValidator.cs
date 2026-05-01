using FluentValidation;

namespace CleanArch.Application.Features.Settings.Commands.Upsert;

public sealed class UpsertSettingCommandValidator : AbstractValidator<UpsertSettingCommand>
{
    public UpsertSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(256).WithMessage("Setting key cannot exceed 256 characters.")
            .Matches(@"^[A-Za-z][A-Za-z0-9_.]*$").WithMessage("Setting key must start with a letter and contain only letters, digits, dots, and underscores.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting value is required.")
            .MaximumLength(4000).WithMessage("Setting value cannot exceed 4000 characters.");

        RuleFor(x => x.Group)
            .NotEmpty().WithMessage("Setting group is required.")
            .MaximumLength(100).WithMessage("Setting group cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.DataType)
            .IsInEnum().WithMessage("Invalid data type.");
    }
}
