using System.Reflection;
using NetArchTest.Rules;

namespace EventsManager.UnitTests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly Domain =
        typeof(EventsManager.Domain.AssemblyMarker).Assembly;

    private static readonly Assembly Application =
        typeof(EventsManager.Application.AssemblyMarker).Assembly;

    private static readonly Assembly Infrastructure =
        typeof(EventsManager.Infrastructure.AssemblyMarker).Assembly;

    [Fact]
    public void Domain_MustNot_DependOn_Application()
    {
        AssertNoDependency(Domain, "EventsManager.Application");
    }

    [Fact]
    public void Domain_MustNot_DependOn_Infrastructure()
    {
        AssertNoDependency(Domain, "EventsManager.Infrastructure");
    }

    [Fact]
    public void Domain_MustNot_DependOn_Api()
    {
        AssertNoDependency(Domain, "EventsManager.Api");
    }

    [Fact]
    public void Application_MustNot_DependOn_Infrastructure()
    {
        AssertNoDependency(Application, "EventsManager.Infrastructure");
    }

    [Fact]
    public void Application_MustNot_DependOn_Api()
    {
        AssertNoDependency(Application, "EventsManager.Api");
    }

    [Fact]
    public void Infrastructure_MustNot_DependOn_Api()
    {
        AssertNoDependency(Infrastructure, "EventsManager.Api");
    }

    private static void AssertNoDependency(Assembly assembly, string forbiddenNamespace)
    {
        var result = Types.InAssembly(assembly)
                          .ShouldNot()
                          .HaveDependencyOnAny(forbiddenNamespace)
                          .GetResult();

        result.IsSuccessful.Should().BeTrue(
            $"{assembly.GetName().Name} ne doit pas dépendre de {forbiddenNamespace}. " +
            $"Types en faute : {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
    }
}
