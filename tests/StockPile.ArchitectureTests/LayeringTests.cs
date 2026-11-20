using System.Reflection;

using NetArchTest.Rules;

using StockPile.Application;
using StockPile.Domain;
using StockPile.Infrastructure;

namespace StockPile.ArchitectureTests;

/// <summary>
/// Enforces the layering: Api -> Application -> Domain; Infrastructure implements Application abstractions.
/// </summary>
public sealed class LayeringTests
{
    private const string ApplicationNamespace = "StockPile.Application";
    private const string InfrastructureNamespace = "StockPile.Infrastructure";
    private const string ApiNamespace = "StockPile.Api";

    private static readonly Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(InfrastructureAssemblyMarker).Assembly;

    [Fact]
    public void Domain_does_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
        AssertNoAssemblyReferences(DomainAssembly, ApplicationNamespace, InfrastructureNamespace, ApiNamespace);
    }

    [Fact]
    public void Domain_does_not_depend_on_persistence_or_web_frameworks()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "Npgsql")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
        AssertNoAssemblyReferences(ApplicationAssembly, InfrastructureNamespace, ApiNamespace);
    }

    [Fact]
    public void Infrastructure_does_not_depend_on_api()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        AssertSuccessful(result);
        AssertNoAssemblyReferences(InfrastructureAssembly, ApiNamespace);
    }

    private static void AssertSuccessful(TestResult result)
    {
        var offenders = result.FailingTypeNames ?? [];
        Assert.True(result.IsSuccessful, $"Layering violated by: {string.Join(", ", offenders)}");
    }

    private static void AssertNoAssemblyReferences(Assembly assembly, params string[] forbidden)
    {
        var references = assembly.GetReferencedAssemblies()
            .Select(a => a.Name)
            .Where(name => name is not null && forbidden.Contains(name))
            .ToList();

        Assert.Empty(references);
    }
}
