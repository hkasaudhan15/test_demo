using FluentValidation;

namespace CleanArch.Application.Features.Tenants.Commands.UpdateProfile;

public sealed class UpdateTenantProfileCommandValidator : AbstractValidator<UpdateTenantProfileCommand>
{
    public UpdateTenantProfileCommandValidator()
    {
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

        RuleFor(x => x.LogoUrl)
            .MaximumLength(2000).WithMessage("Logo URL cannot exceed 2000 characters.");

        RuleFor(x => x.CustomDomain)
            .MaximumLength(256).WithMessage("Custom domain cannot exceed 256 characters.")
            .Matches(@"^([a-z0-9]+(-[a-z0-9]+)*\.)+[a-z]{2,}$")
            .When(x => !string.IsNullOrEmpty(x.CustomDomain))
            .WithMessage("Invalid domain format.");
    }
}
