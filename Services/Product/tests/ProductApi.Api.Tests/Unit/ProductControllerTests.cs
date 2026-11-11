using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductApi.Api.Controllers;
using ProductApi.Api.Interfaces;
using ProductApi.Api.Models;

namespace ProductApi.Api.Tests.Unit;

public class ProductControllerTests
{
    private readonly Mock<IProductService> _productService = new();
    private readonly ProductController _controller;
    
    public ProductControllerTests()
    {        
        _controller = new ProductController(_productService.Object, NullLogger<ProductController>.Instance);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task Delete_ValidIdAndCategory_ReturnsNoContent(ProductCategory category)
    {
        var id = Guid.NewGuid();
        
        var result = await _controller.Delete(id, category);
        
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, statusResult.StatusCode);
        _productService.Verify(service => service.Delete(id, category), Times.Once);
    }
    
    [Theory]
    [InlineData(ProductCategory.Cpu)]
    [InlineData(ProductCategory.Gpu)]
    [InlineData(ProductCategory.Cooler)]
    public async Task Delete_EmptyId_ReturnsBadRequest(ProductCategory category)
    {
        var result = await _controller.Delete(Guid.Empty, category);
        
        Assert.IsType<BadRequestObjectResult>(result);
        
        _productService.Verify(service => service.Delete(It.IsAny<Guid>(), It.IsAny<ProductCategory>()), Times.Never);
    }
}