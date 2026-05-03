using CleanArch.BddTests.Support;
using CleanArch.Domain.Tenants;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CleanArch.BddTests.StepDefinitions;

[Binding]
public sealed class TenantProfileSteps
{
    private readonly TestContext _context;

    public TenantProfileSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"a tenant ""(.*)"" exists with name ""(.*)"" and email ""(.*)""")]
    public void GivenATenantExistsWithNameAndEmail(string identifier, string name, string email)
    {
        var tenant = Tenant.Register(identifier, name, email);
        tenant.Activate();
        tenant.ClearDomainEvents();
        _context.CurrentTenant = tenant;
    }

    [Given(@"the tenant has custom domain ""(.*)""")]
    public void GivenTheTenantHasCustomDomain(string domain)
    {
        _context.CurrentTenant!.SetCustomDomain(domain);
    }

    [When(@"I update the tenant profile:")]
    public void WhenIUpdateTheTenantProfile(Table table)
    {
        var row = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        var name = row.GetValueOrDefault("Name") ?? _context.CurrentTenant!.Name;
        var email = row.GetValueOrDefault("ContactEmail") ?? _context.CurrentTenant!.ContactEmail;
        var adminName = row.GetValueOrDefault("AdminName");
        var description = row.GetValueOrDefault("Description");

        _context.CurrentTenant!.UpdateProfile(name, description, email, adminName);
    }

    [When(@"I set the custom domain to ""(.*)""")]
    public void WhenISetTheCustomDomainTo(string domain)
    {
        _context.CurrentTenant!.SetCustomDomain(domain);
    }

    [When(@"I set the custom domain to null")]
    public void WhenISetTheCustomDomainToNull()
    {
        _context.CurrentTenant!.SetCustomDomain(null);
    }

    [When(@"I set the logo URL to ""(.*)""")]
    public void WhenISetTheLogoUrlTo(string logoUrl)
    {
        _context.CurrentTenant!.SetLogoUrl(logoUrl);
    }

    [When(@"I set the connection string to ""(.*)""")]
    public void WhenISetTheConnectionStringTo(string connectionString)
    {
        _context.CurrentTenant!.SetConnectionString(connectionString);
    }

    [When(@"I attempt to update the tenant profile with empty name")]
    public void WhenIAttemptToUpdateTheTenantProfileWithEmptyName()
    {
        try
        {
            _context.CurrentTenant!.UpdateProfile(string.Empty, null, "admin@test.com", null);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [When(@"I attempt to update the tenant profile with empty email")]
    public void WhenIAttemptToUpdateTheTenantProfileWithEmptyEmail()
    {
        try
        {
            _context.CurrentTenant!.UpdateProfile("Valid Name", null, string.Empty, null);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the tenant description should be ""(.*)""")]
    public void ThenTheTenantDescriptionShouldBe(string expected)
    {
        _context.CurrentTenant!.Description.Should().Be(expected);
    }

    [Then(@"the tenant custom domain should be ""(.*)""")]
    public void ThenTheTenantCustomDomainShouldBe(string expected)
    {
        _context.CurrentTenant!.CustomDomain.Should().Be(expected);
    }

    [Then(@"the tenant custom domain should be null")]
    public void ThenTheTenantCustomDomainShouldBeNull()
    {
        _context.CurrentTenant!.CustomDomain.Should().BeNull();
    }

    [Then(@"the tenant logo URL should be ""(.*)""")]
    public void ThenTheTenantLogoUrlShouldBe(string expected)
    {
        _context.CurrentTenant!.LogoUrl.Should().Be(expected);
    }

    [Then(@"the tenant connection string should be ""(.*)""")]
    public void ThenTheTenantConnectionStringShouldBe(string expected)
    {
        _context.CurrentTenant!.ConnectionString.Should().Be(expected);
    }
}
