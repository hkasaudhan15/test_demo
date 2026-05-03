using CleanArch.Application.Abstractions.Tenants;
using CleanArch.Application.Features.Tenants.Commands.Register;
using CleanArch.BddTests.Support;
using CleanArch.Domain.Abstractions.Events;
using CleanArch.Domain.Tenants;
using CleanArch.Domain.Tenants.Events;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CleanArch.BddTests.StepDefinitions;

[Binding]
public sealed class TenantRegistrationSteps : IDisposable
{
    private readonly TestContext _context;
    private readonly InMemoryTenantRepository _repository;
    private readonly InMemoryUnitOfWork _unitOfWork;
    private bool _autoActivate = true;

    public TenantRegistrationSteps(TestContext context)
    {
        _context = context;
        _repository = new InMemoryTenantRepository();
        _unitOfWork = new InMemoryUnitOfWork();
    }

    [Given(@"the tenant repository is empty")]
    public void GivenTheTenantRepositoryIsEmpty()
    {
        // Repository starts empty by default — no-op, validates precondition
        _ = _repository;
    }

    public void Dispose() => _unitOfWork.Dispose();

    [Given(@"I have tenant registration details:")]
    public void GivenIHaveTenantRegistrationDetails(Table table)
    {
        var row = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        _context.InputIdentifier = row.GetValueOrDefault("Identifier") ?? string.Empty;
        _context.InputName = row.GetValueOrDefault("Name") ?? string.Empty;
        _context.InputEmail = row.GetValueOrDefault("ContactEmail") ?? string.Empty;
        _context.InputAdminName = row.GetValueOrDefault("AdminName");
        _context.InputDescription = row.GetValueOrDefault("Description");
    }

    [Given(@"the subscription tier is ""(.*)""")]
    public void GivenTheSubscriptionTierIs(string tier)
    {
        _context.InputTier = Enum.Parse<SubscriptionTier>(tier);
    }

    [Given(@"auto-activation is disabled")]
    public void GivenAutoActivationIsDisabled()
    {
        _autoActivate = false;
    }

    [Given(@"a tenant with identifier ""(.*)"" already exists")]
    public void GivenATenantWithIdentifierAlreadyExists(string identifier)
    {
        var tenant = Tenant.Register(identifier, "Existing Tenant", "existing@test.com");
        tenant.Activate();
        _repository.Add(tenant);
    }

    [When(@"I register the tenant")]
    public void WhenIRegisterTheTenant()
    {
        var handler = new RegisterTenantCommandHandler(_repository, _unitOfWork);
        var command = new RegisterTenantCommand(
            _context.InputIdentifier!,
            _context.InputName!,
            _context.InputEmail!,
            _context.InputAdminName,
            _context.InputDescription,
            _context.InputTier,
            _autoActivate);

        var result = handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
        _context.LastResult = result.IsSuccess
            ? CleanArch.Domain.Primitives.Results.Result.Success()
            : CleanArch.Domain.Primitives.Results.Result.Failure(result.Error);
        if (result.IsSuccess)
        {
            _context.LastCreatedId = result.Value;
            _context.CurrentTenant = _repository.GetByIdAsync(result.Value).GetAwaiter().GetResult();
        }
    }

    [When(@"I attempt to register the tenant")]
    public void WhenIAttemptToRegisterTheTenant()
    {
        try
        {
            var tenant = Tenant.Register(
                _context.InputIdentifier ?? string.Empty,
                _context.InputName ?? string.Empty,
                _context.InputEmail ?? string.Empty,
                _context.InputAdminName,
                _context.InputDescription,
                _context.InputTier);
            _context.CurrentTenant = tenant;
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the tenant should be created successfully")]
    public void ThenTheTenantShouldBeCreatedSuccessfully()
    {
        _context.LastResult.Should().NotBeNull();
        _context.LastResult!.IsSuccess.Should().BeTrue();
        _context.CurrentTenant.Should().NotBeNull();
    }

    [Then(@"the tenant identifier should be ""(.*)""")]
    public void ThenTheTenantIdentifierShouldBe(string expected)
    {
        _context.CurrentTenant!.Identifier.Should().Be(expected);
    }

    [Then(@"the tenant name should be ""(.*)""")]
    public void ThenTheTenantNameShouldBe(string expected)
    {
        _context.CurrentTenant!.Name.Should().Be(expected);
    }

    [Then(@"the tenant subscription tier should be ""(.*)""")]
    public void ThenTheTenantSubscriptionTierShouldBe(string expected)
    {
        var expectedTier = Enum.Parse<SubscriptionTier>(expected);
        _context.CurrentTenant!.Tier.Should().Be(expectedTier);
    }

    [Then(@"the tenant status should be ""(.*)""")]
    public void ThenTheTenantStatusShouldBe(string expected)
    {
        var expectedStatus = Enum.Parse<TenantStatus>(expected);
        _context.CurrentTenant!.Status.Should().Be(expectedStatus);
    }

    [Then(@"a TenantRegistered domain event should be raised")]
    public void ThenATenantRegisteredDomainEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().ContainSingle(e => e is TenantRegisteredDomainEvent);
    }

    [Then(@"a TenantActivated domain event should be raised")]
    public void ThenATenantActivatedDomainEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().Contain(e => e is TenantActivatedDomainEvent);
    }

    [Then(@"only a TenantRegistered domain event should be raised")]
    public void ThenOnlyATenantRegisteredDomainEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().ContainSingle()
            .Which.Should().BeOfType<TenantRegisteredDomainEvent>();
    }

    [Then(@"the tenant max users should be unlimited")]
    public void ThenTheTenantMaxUsersShouldBeUnlimited()
    {
        _context.CurrentTenant!.MaxUsers.Should().Be(int.MaxValue);
    }

    [Then(@"the registration should fail with error ""(.*)""")]
    public void ThenTheRegistrationShouldFailWithError(string errorCode)
    {
        _context.LastResult.Should().NotBeNull();
        _context.LastResult!.IsFailure.Should().BeTrue();
        _context.LastResult!.Error.Code.Should().Be(errorCode);
    }

    [Then(@"it should throw an ArgumentException for ""(.*)""")]
    public void ThenItShouldThrowAnArgumentExceptionFor(string paramName)
    {
        _context.LastException.Should().NotBeNull();
        _context.LastException.Should().BeOfType<ArgumentException>();
        ((ArgumentException)_context.LastException!).ParamName.Should().Be(paramName);
    }

    [Then(@"the tenant contact email should be ""(.*)""")]
    public void ThenTheTenantContactEmailShouldBe(string expected)
    {
        _context.CurrentTenant!.ContactEmail.Should().Be(expected);
    }
}
