using CleanArch.BddTests.Support;
using CleanArch.Domain.Tenants;
using CleanArch.Domain.Tenants.Events;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CleanArch.BddTests.StepDefinitions;

[Binding]
public sealed class TenantLifecycleSteps
{
    private readonly TestContext _context;
    private readonly InMemoryTenantRepository _repository;
    private int _initialEventCount;

    public TenantLifecycleSteps(TestContext context)
    {
        _context = context;
        _repository = new InMemoryTenantRepository();
    }

    [Given(@"a tenant ""(.*)"" exists with status ""(.*)""")]
    public void GivenATenantExistsWithStatus(string identifier, string status)
    {
        var tenant = Tenant.Register(identifier, $"{identifier} Corp", $"admin@{identifier}.com");

        switch (Enum.Parse<TenantStatus>(status))
        {
            case TenantStatus.Active:
                tenant.Activate();
                break;
            case TenantStatus.Suspended:
                tenant.Activate();
                tenant.Suspend("Test suspension");
                break;
            case TenantStatus.Deactivated:
                tenant.Activate();
                tenant.Deactivate("Test deactivation");
                break;
        }

        tenant.ClearDomainEvents();
        _repository.Add(tenant);
        _context.CurrentTenant = tenant;
    }

    [Given(@"the tenant status is ""(.*)""")]
    public void GivenTheTenantStatusIs(string status)
    {
        var tenant = _context.CurrentTenant!;
        tenant.ClearDomainEvents();

        switch (Enum.Parse<TenantStatus>(status))
        {
            case TenantStatus.Provisioning:
                // Create a fresh provisioning tenant
                var provTenant = Tenant.Register("prov-tenant", "Provisioning Tenant", "admin@prov.com");
                provTenant.ClearDomainEvents();
                _context.CurrentTenant = provTenant;
                break;
        }
    }

    [Given(@"the tenant is suspended with reason ""(.*)""")]
    public void GivenTheTenantIsSuspendedWithReason(string reason)
    {
        _context.CurrentTenant!.Suspend(reason);
        _context.CurrentTenant!.ClearDomainEvents();
    }

    [Given(@"the tenant is deactivated with reason ""(.*)""")]
    public void GivenTheTenantIsDeactivatedWithReason(string reason)
    {
        _context.CurrentTenant!.Deactivate(reason);
        _context.CurrentTenant!.ClearDomainEvents();
    }

    [Given(@"the following tenants exist:")]
    public void GivenTheFollowingTenantsExist(Table table)
    {
        _context.TenantCollection.Clear();
        foreach (var row in table.Rows)
        {
            var identifier = row["Identifier"];
            var status = Enum.Parse<TenantStatus>(row["Status"]);
            var tenant = Tenant.Register(identifier, $"{identifier} Corp", $"admin@{identifier}.com");
            tenant.Activate();

            switch (status)
            {
                case TenantStatus.Suspended:
                    tenant.Suspend("Test reason");
                    break;
                case TenantStatus.Deactivated:
                    tenant.Deactivate("Test reason");
                    break;
            }

            _repository.Add(tenant);
            _context.TenantCollection.Add(tenant);
        }
    }

    [When(@"I activate the tenant")]
    public void WhenIActivateTheTenant()
    {
        _initialEventCount = _context.CurrentTenant!.DomainEvents.Count;
        _context.CurrentTenant!.Activate();
    }

    [When(@"I suspend the tenant with reason ""(.*)""")]
    public void WhenISuspendTheTenantWithReason(string reason)
    {
        _context.CurrentTenant!.Suspend(reason);
    }

    [When(@"I attempt to suspend the tenant without a reason")]
    public void WhenIAttemptToSuspendTheTenantWithoutAReason()
    {
        try
        {
            _context.CurrentTenant!.Suspend(string.Empty);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [When(@"I deactivate the tenant with reason ""(.*)""")]
    public void WhenIDeactivateTheTenantWithReason(string reason)
    {
        _context.CurrentTenant!.Deactivate(reason);
    }

    [When(@"I attempt to deactivate the tenant without a reason")]
    public void WhenIAttemptToDeactivateTheTenantWithoutAReason()
    {
        try
        {
            _context.CurrentTenant!.Deactivate(string.Empty);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [When(@"I query tenants with status ""(.*)""")]
    public void WhenIQueryTenantsWithStatus(string status)
    {
        var parsedStatus = Enum.Parse<TenantStatus>(status);
        var result = _repository.GetByStatusAsync(parsedStatus).GetAwaiter().GetResult();
        _context.TenantCollection = result.ToList();
    }

    [Then(@"the activation date should be set")]
    public void ThenTheActivationDateShouldBeSet()
    {
        _context.CurrentTenant!.ActivatedOnUtc.Should().NotBeNull();
        _context.CurrentTenant!.ActivatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Then(@"no additional domain events should be raised")]
    public void ThenNoAdditionalDomainEventsShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents.Count.Should().Be(_initialEventCount);
    }

    [Then(@"the suspension reason should be cleared")]
    public void ThenTheSuspensionReasonShouldBeCleared()
    {
        _context.CurrentTenant!.SuspensionReason.Should().BeNull();
    }

    [Then(@"the suspension date should be set")]
    public void ThenTheSuspensionDateShouldBeSet()
    {
        _context.CurrentTenant!.SuspendedOnUtc.Should().NotBeNull();
        _context.CurrentTenant!.SuspendedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Then(@"the suspension reason should be ""(.*)""")]
    public void ThenTheSuspensionReasonShouldBe(string expected)
    {
        _context.CurrentTenant!.SuspensionReason.Should().Be(expected);
    }

    [Then(@"a TenantDeactivated domain event should be raised")]
    public void ThenATenantDeactivatedDomainEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().Contain(e => e is TenantDeactivatedDomainEvent);
    }

    [Then(@"I should get (\d+) tenants")]
    public void ThenIShouldGetTenants(int count)
    {
        _context.TenantCollection.Should().HaveCount(count);
    }

    [Then(@"the results should contain ""(.*)"" and ""(.*)""")]
    public void ThenTheResultsShouldContainAnd(string id1, string id2)
    {
        _context.TenantCollection.Select(t => t.Identifier).Should().Contain(id1);
        _context.TenantCollection.Select(t => t.Identifier).Should().Contain(id2);
    }
}
