using CleanArch.BddTests.Support;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace CleanArch.BddTests.StepDefinitions;

[Binding]
public sealed class TenantFeatureFlagsSteps
{
    private readonly TestContext _context;
    private bool _hasFeatureResult;

    public TenantFeatureFlagsSteps(TestContext context)
    {
        _context = context;
    }

    [Given(@"the tenant has feature ""(.*)"" enabled")]
    public void GivenTheTenantHasFeatureEnabled(string featureCode)
    {
        _context.CurrentTenant!.EnableFeature(featureCode);
    }

    [Given(@"the tenant has feature ""(.*)"" disabled")]
    public void GivenTheTenantHasFeatureDisabled(string featureCode)
    {
        _context.CurrentTenant!.EnableFeature(featureCode);
        _context.CurrentTenant!.DisableFeature(featureCode);
    }

    [Given(@"the tenant has feature ""(.*)"" with past expiry")]
    public void GivenTheTenantHasFeatureWithPastExpiry(string featureCode)
    {
        _context.CurrentTenant!.EnableFeature(featureCode, DateTime.UtcNow.AddDays(-1));
    }

    [When(@"I enable feature ""(.*)"" for the tenant")]
    public void WhenIEnableFeatureForTheTenant(string featureCode)
    {
        _context.CurrentTenant!.EnableFeature(featureCode);
    }

    [When(@"I enable feature ""(.*)"" with expiry ""(.*)""")]
    public void WhenIEnableFeatureWithExpiry(string featureCode, string expiryDate)
    {
        var expiry = DateTime.Parse(expiryDate).ToUniversalTime();
        _context.CurrentTenant!.EnableFeature(featureCode, expiry);
    }

    [When(@"I disable feature ""(.*)"" for the tenant")]
    public void WhenIDisableFeatureForTheTenant(string featureCode)
    {
        _context.CurrentTenant!.DisableFeature(featureCode);
    }

    [When(@"I check if the tenant has feature ""(.*)""")]
    public void WhenICheckIfTheTenantHasFeature(string featureCode)
    {
        _hasFeatureResult = _context.CurrentTenant!.HasFeature(featureCode);
    }

    [When(@"I attempt to enable feature ""(.*)"" for the tenant")]
    public void WhenIAttemptToEnableFeatureForTheTenant(string featureCode)
    {
        try
        {
            _context.CurrentTenant!.EnableFeature(featureCode);
        }
        catch (Exception ex)
        {
            _context.LastException = ex;
        }
    }

    [Then(@"the tenant should have feature ""(.*)"" enabled")]
    public void ThenTheTenantShouldHaveFeatureEnabled(string featureCode)
    {
        _context.CurrentTenant!.HasFeature(featureCode).Should().BeTrue();
    }

    [Then(@"the tenant should not have feature ""(.*)"" active")]
    public void ThenTheTenantShouldNotHaveFeatureActive(string featureCode)
    {
        _context.CurrentTenant!.HasFeature(featureCode).Should().BeFalse();
    }

    [Then(@"the tenant should have (\d+) features? configured")]
    public void ThenTheTenantShouldHaveNFeaturesConfigured(int count)
    {
        _context.CurrentTenant!.Features.Should().HaveCount(count);
    }

    [Then(@"the feature ""(.*)"" should expire on ""(.*)""")]
    public void ThenTheFeatureShouldExpireOn(string featureCode, string expectedDate)
    {
        var expected = DateTime.Parse(expectedDate).ToUniversalTime();
        var feature = _context.CurrentTenant!.Features.First(f => f.FeatureCode == featureCode);
        feature.ExpiresOnUtc.Should().NotBeNull();
        feature.ExpiresOnUtc!.Value.Date.Should().Be(expected.Date);
    }

    [Then(@"the result should be false")]
    public void ThenTheResultShouldBeFalse()
    {
        _hasFeatureResult.Should().BeFalse();
    }

    [Then(@"the result should be true")]
    public void ThenTheResultShouldBeTrue()
    {
        _hasFeatureResult.Should().BeTrue();
    }
}
