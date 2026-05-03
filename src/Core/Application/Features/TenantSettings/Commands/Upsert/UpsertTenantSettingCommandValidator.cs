using FluentValidation;

namespace CleanArch.Application.Features.TenantSettings.Commands.Upsert;

public sealed class UpsertTenantSettingCommandValidator : AbstractValidator<UpsertTenantSettingCommand>
{
    public UpsertTenantSettingCommandValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Setting key is required.")
            .MaximumLength(256).WithMessage("Setting key cannot exceed 256 characters.");

        RuleFor(x => x.Value)
            .NotNull().WithMessage("Setting value is required.")
            .MaximumLength(4000).WithMessage("Setting value cannot exceed 4000 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
