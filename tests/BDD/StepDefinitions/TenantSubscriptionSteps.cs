using CleanArch.BddTests.Support;
using CleanArch.Domain.Tenants;
using CleanArch.Domain.Tenants.Events;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CleanArch.BddTests.StepDefinitions;

[Binding]
public sealed class TenantSubscriptionSteps
{
    private readonly TestContext _context;

    public TenantSubscriptionSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"a tenant ""(.*)"" exists with tier ""(.*)""")]
    public void GivenATenantExistsWithTier(string identifier, string tier)
    {
        var parsedTier = Enum.Parse<SubscriptionTier>(tier);
        var tenant = Tenant.Register(identifier, $"{identifier} Corp", $"admin@{identifier}.com", tier: parsedTier);
        tenant.Activate();
        tenant.ClearDomainEvents();
        _context.CurrentTenant = tenant;
    }

    [Given(@"the tenant tier is ""(.*)""")]
    public void GivenTheTenantTierIs(string tier)
    {
        var parsedTier = Enum.Parse<SubscriptionTier>(tier);
        _context.CurrentTenant!.ChangeSubscription(parsedTier);
        _context.CurrentTenant!.ClearDomainEvents();
    }

    [When(@"I change the subscription to ""([^""]*)""$")]
    public void WhenIChangeTheSubscriptionTo(string tier)
    {
        var parsedTier = Enum.Parse<SubscriptionTier>(tier);
        _context.CurrentTenant!.ChangeSubscription(parsedTier);
    }

    [When(@"I change the subscription to ""([^""]*)"" with expiry ""([^""]*)""")]
    public void WhenIChangeTheSubscriptionToWithExpiry(string tier, string expiryDate)
    {
        var parsedTier = Enum.Parse<SubscriptionTier>(tier);
        var expiry = DateTime.Parse(expiryDate).ToUniversalTime();
        _context.CurrentTenant!.ChangeSubscription(parsedTier, expiry);
    }

    [Then(@"the tenant max users should be (\d+)")]
    public void ThenTheTenantMaxUsersShouldBe(int expected)
    {
        _context.CurrentTenant!.MaxUsers.Should().Be(expected);
    }

    [Then(@"the tenant max storage should be (\d+) MB")]
    public void ThenTheTenantMaxStorageShouldBe(int expected)
    {
        _context.CurrentTenant!.MaxStorageMb.Should().Be(expected);
    }

    [Then(@"the tenant max storage should be unlimited")]
    public void ThenTheTenantMaxStorageShouldBeUnlimited()
    {
        _context.CurrentTenant!.MaxStorageMb.Should().Be(int.MaxValue);
    }

    [Then(@"the tenant max API calls should be (\d+)")]
    public void ThenTheTenantMaxApiCallsShouldBe(int expected)
    {
        _context.CurrentTenant!.MaxApiCallsPerDay.Should().Be(expected);
    }

    [Then(@"the tenant max API calls should be unlimited")]
    public void ThenTheTenantMaxApiCallsShouldBeUnlimited()
    {
        _context.CurrentTenant!.MaxApiCallsPerDay.Should().Be(int.MaxValue);
    }

    [Then(@"the tenant max workspaces should be (\d+)")]
    public void ThenTheTenantMaxWorkspacesShouldBe(int expected)
    {
        _context.CurrentTenant!.MaxProjectsOrWorkspaces.Should().Be(expected);
    }

    [Then(@"a TenantSubscriptionChanged domain event should be raised")]
    public void ThenATenantSubscriptionChangedDomainEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().Contain(e => e is TenantSubscriptionChangedDomainEvent);
    }

    [Then(@"no subscription changed event should be raised")]
    public void ThenNoSubscriptionChangedEventShouldBeRaised()
    {
        _context.CurrentTenant!.DomainEvents
            .Should().NotContain(e => e is TenantSubscriptionChangedDomainEvent);
    }

    [Then(@"the subscription expiry date should be ""(.*)""")]
    public void ThenTheSubscriptionExpiryDateShouldBe(string expected)
    {
        var expectedDate = DateTime.Parse(expected).ToUniversalTime();
        _context.CurrentTenant!.SubscriptionExpiresOnUtc.Should().NotBeNull();
        _context.CurrentTenant!.SubscriptionExpiresOnUtc!.Value.Date.Should().Be(expectedDate.Date);
    }

    [Then(@"the tenant should not be expired")]
    public void ThenTheTenantShouldNotBeExpired()
    {
        _context.CurrentTenant!.IsSubscriptionExpired.Should().BeFalse();
    }

    [Then(@"the tenant should be expired")]
    public void ThenTheTenantShouldBeExpired()
    {
        _context.CurrentTenant!.IsSubscriptionExpired.Should().BeTrue();
    }
}
