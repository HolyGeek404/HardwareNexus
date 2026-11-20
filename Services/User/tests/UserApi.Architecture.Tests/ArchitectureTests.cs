using NetArchTest.Rules;
using UserApi.Application;
using UserApi.Domain;
using UserApi.Infrastructure;

namespace UserApi.Architecture.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnOthers()
    {
        var result = Types.InAssembly(typeof(DomainAssemblyMarker).Assembly)
            .Should()
            .OnlyHaveDependenciesOn("UserApi.Domain","System")
            .GetResult();
        Assert.True(result.IsSuccessful);
    }
    
    [Fact]
    public void Application_ShouldNotDependOn_Presentation_Infra()
    {
        var result = Types.InAssembly(typeof(ApplicationAssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("UserApi.Infrastructure","UserApi.Presentation")
            .GetResult();
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOn_Presentation()
    {
        var result = Types
            .InAssembly(typeof(InfrastructureAssemblyMarker).Assembly)   
            .ShouldNot()
            .HaveDependencyOn("OrderApi.Presentation")
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames?.ToString());

    }
}