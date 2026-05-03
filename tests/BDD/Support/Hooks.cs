using TechTalk.SpecFlow;

namespace CleanArch.BddTests.Support;

/// <summary>
/// SpecFlow hooks for test lifecycle management.
/// </summary>
[Binding]
public sealed class Hooks
{
    private readonly TestContext _context;

    public Hooks(TestContext context)
    {
        _context = context;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        // Context is automatically fresh per scenario via SpecFlow DI
        _ = _context;
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _context.CurrentTenant = null;
        _context.TenantCollection.Clear();
        _context.LastResult = null;
        _context.LastException = null;
    }
}
