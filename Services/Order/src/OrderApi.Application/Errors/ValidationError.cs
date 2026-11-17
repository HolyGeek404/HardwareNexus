using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Errors;

public sealed record ValidationError(IReadOnlyList<Error> Errors)
    : Error("Validation.Failed", "One or more validation errors occurred.", ErrorType.Validation);
