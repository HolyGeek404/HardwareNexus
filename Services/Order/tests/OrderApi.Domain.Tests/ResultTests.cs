using OrderApi.Domain.Primitives;

namespace OrderApi.Domain.Tests;

public class ResultTests
{
    private static readonly Error SampleError = Error.Validation("Sample.Error", "Sample message");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ContainsError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Failure_WithErrorNone_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Error.None));
    }

    [Fact]
    public void SuccessWithValue_ExposesValue()
    {
        var result = Result.Success(42);

        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void FailureWithValue_AccessingValue_Throws()
    {
        var result = Result.Failure<int>(SampleError);

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void ImplicitConversions_FromValueAndError()
    {
        Result<string> success = "ok";
        Result<string> failure = SampleError;

        Assert.True(success.IsSuccess);
        Assert.Equal("ok", success.Value);
        Assert.True(failure.IsFailure);
        Assert.Equal(SampleError, failure.Error);
    }
}
