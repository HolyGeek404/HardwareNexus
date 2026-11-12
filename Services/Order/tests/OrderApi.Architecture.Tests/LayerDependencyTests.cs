using NetArchTest.Rules;
using OrderApi.Application;
using OrderApi.Domain;
using OrderApi.Infrastructure;

namespace OrderApi.Architecture.Tests;

public class LayerDependencyTests
{
    [Fact]
    public void Domain_ShouldOnlyDependOn_SystemAndItself()
    {
        var result = Types
            .InAssembly(typeof(DomainAssemblyMarker).Assembly)   
            .Should()
            .OnlyHaveDependenciesOn(["OrderApi.Domain","System"])
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames?.ToString());
    }
    
    [Theory]
    [InlineData("OrderApi.Infrastructure")]
    [InlineData("OrderApi.Presentation")]
    public void Application_ShouldNotDependOn_Presentation_Infra(string namespaceName)
    {
        var result = Types
            .InAssembly(typeof(ApplicationAssemblyMarker).Assembly)   
            .ShouldNot()
            .HaveDependencyOn(namespaceName)
            .GetResult();

        Assert.True(result.IsSuccessful, result.FailingTypeNames?.ToString());
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