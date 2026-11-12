using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductApi.Api.Extensions;

namespace ProductApi.Api.Tests.Unit;

public class GlobalExceptionHandlerTest
{
    private readonly Mock<IProblemDetailsService> _problemDetailsServiceMock = new();
    private readonly GlobalExceptionHandler _globalExceptionHandler;

    public GlobalExceptionHandlerTest()
    {
        _globalExceptionHandler = new GlobalExceptionHandler(_problemDetailsServiceMock.Object, NullLogger<GlobalExceptionHandler>.Instance);
    }

    [Theory, MemberData(nameof(ExceptionMappings))]
    public async Task TryHandleAsync_MapsExceptionToStatusCode(Exception exception, int expectedStatusCode)
    {
        var httpContext = new DefaultHttpContext();

       await _globalExceptionHandler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);
    }
    
    public static TheoryData<Exception, int> ExceptionMappings => new()
    {
        { new NotImplementedException(), StatusCodes.Status501NotImplemented },
        { new InvalidOperationException(), StatusCodes.Status500InternalServerError },
        { new System.Text.Json.JsonException(), StatusCodes.Status400BadRequest },
    };
}