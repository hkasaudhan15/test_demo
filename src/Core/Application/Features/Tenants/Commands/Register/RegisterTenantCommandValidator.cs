using FluentValidation;

namespace CleanArch.Application.Features.Tenants.Commands.Register;

public sealed class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.Identifier)
            .NotEmpty().WithMessage("Tenant identifier is required.")
            .MaximumLength(100).WithMessage("Identifier cannot exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9\-]*[a-z0-9])?$")
            .WithMessage("Identifier must be lowercase alphanumeric with hyphens (slug format, e.g., 'acme-corp').");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(256).WithMessage("Name cannot exceed 256 characters.");

        RuleFor(x => x.ContactEmail)
            .NotEmpty().WithMessage("Contact email is required.")
            .EmailAddress().WithMessage("Invalid email address format.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        RuleFor(x => x.AdminName)
            .MaximumLength(256).WithMessage("Admin name cannot exceed 256 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.Tier)
            .IsInEnum().WithMessage("Invalid subscription tier.");
    }
}
