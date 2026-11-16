using OrderApi.Domain.Primitives;

namespace OrderApi.Application.Errors;

public static class ApplicationErrors
{
    public static class Product
    {
        public static readonly Error NotFound =
            Error.NotFound("Product.NotFound", "Product does not exist in the catalog.");
    }
}
