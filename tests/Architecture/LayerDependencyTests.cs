using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace CleanArch.ArchitectureTests;

/// <summary>
/// Enforces Clean Architecture dependency rules at compile time.
/// These tests FAIL the build if someone violates layer boundaries.
/// </summary>
public class LayerDependencyTests
{
    private const string DomainNamespace = "CleanArch.Domain";
    private const string ApplicationNamespace = "CleanArch.Application";
    private const string InfrastructureNamespace = "CleanArch.Infrastructure";
    private const string PresentationNamespace = "CleanArch.Presentation";

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(typeof(Domain.Abstractions.Entities.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must NEVER depend on Application layer.");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Domain.Abstractions.Entities.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must NEVER depend on Infrastructure layer.");
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Presentation()
    {
        var result = Types.InAssembly(typeof(Domain.Abstractions.Entities.Entity).Assembly)
            .ShouldNot()
            .HaveDependencyOn(PresentationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must NEVER depend on Presentation layer.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application must NEVER depend on Infrastructure layer.");
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Presentation()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(PresentationNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application must NEVER depend on Presentation layer.");
    }
}
