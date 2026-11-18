using Microsoft.EntityFrameworkCore;
using OrderApi.Infrastructure.Persistence;

namespace OrderApi.Infrastructure.Tests;

public class OrderDbContextModelTests
{
    private static string GenerateCreateScript()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlServer("Server=unused;Database=unused")
            .Options;

        using var db = new OrderDbContext(options);
        return db.Database.GenerateCreateScript();
    }

    [Fact]
    public void Model_IsValid_AndMapsOrderAggregateToTwoTables()
    {
        var script = GenerateCreateScript();

        Assert.Contains("CREATE TABLE [Orders]", script);
        Assert.Contains("CREATE TABLE [OrderLines]", script);
    }

    [Theory]
    [InlineData("[TotalAmount] decimal(18,2)")]   
    [InlineData("[Status] nvarchar(20)")]         
    [InlineData("[UnitPrice] decimal(18,2)")]
    [InlineData("[Quantity] int")]
    [InlineData("PRIMARY KEY ([OrderId], [ProductId])")]
    public void Script_ContainsExpectedColumns(string fragment)
    {
        Assert.Contains(fragment, GenerateCreateScript());
    }

    [Fact]
    public void DerivedAndDomainOnlyMembers_AreNotMapped()
    {
        var script = GenerateCreateScript();

        Assert.DoesNotContain("TotalPrice", script);
        Assert.DoesNotContain("DomainEvents", script);
    }
}
