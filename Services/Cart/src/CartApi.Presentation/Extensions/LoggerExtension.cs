using CartApi.Presentation.Controllers;

namespace CartApi.Presentation.Extensions;

public static partial class LoggerExtension
{
    [LoggerMessage(LogLevel.Warning, "Validation failed while adding item to cart {userId}")]
    public static partial void LogValidationFailedWhileAddingItemToCartUserId(this ILogger<CartController> logger,
        Exception ex, string userId);

    [LoggerMessage(LogLevel.Error, "Unexpected error while adding item to cart {userId}")]
    public static partial void LogUnexpectedErrorWhileAddingItemToCartUserId(this ILogger<CartController> logger,
        Exception ex, string userId);

    [LoggerMessage(LogLevel.Warning, "Validation failed while retrieving cart {userId}")]
    public static partial void LogValidationFailedWhileRetrievingCartUserid(this ILogger<CartController> logger,
        Exception ex, string userId);

    [LoggerMessage(LogLevel.Error, "Unexpected error while retrieving cart {userId}")]
    public static partial void LogUnexpectedErrorWhileRetrievingCartUserid(this ILogger<CartController> logger,
        Exception ex, string userId);

    [LoggerMessage(LogLevel.Warning, "Validation failed while removing item {productId} from cart {userId}")]
    public static partial void LogValidationFailedWhileRemovingItemFromCart(this ILogger<CartController> logger,
        Exception ex, string userId, string productId);

    [LoggerMessage(LogLevel.Error, "Unexpected error while removing item {productId} from cart {userId}")]
    public static partial void LogUnexpectedErrorWhileRemovingItemFromCart(this ILogger<CartController> logger,
        Exception ex, string userId, string productId);
}