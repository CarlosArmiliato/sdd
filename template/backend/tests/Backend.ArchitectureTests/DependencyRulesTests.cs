using Backend.App;
using Backend.Domain.Health;

namespace Backend.ArchitectureTests;

public sealed class DependencyRulesTests
{
    [Fact]
    public void DomainDoesNotReferenceApplicationOrInfrastructure()
    {
        string[] references = typeof(ServiceHealth).Assembly.GetReferencedAssemblies().Select(x => x.Name!).ToArray();
        Assert.DoesNotContain(references, name => name.StartsWith("Backend.App", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Backend.Infra", StringComparison.Ordinal));
    }

    [Fact]
    public void ApplicationDoesNotReferenceInfrastructure()
    {
        string[] references = typeof(AppAssemblyMarker).Assembly.GetReferencedAssemblies().Select(x => x.Name!).ToArray();
        Assert.DoesNotContain(references, name => name.StartsWith("Backend.Infra", StringComparison.Ordinal));
    }
}
