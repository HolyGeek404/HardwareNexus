using CartApi.Application.Features.Commands.AddCart;
using CartApi.Application.Features.Commands.RemoveItem;
using CartApi.Application.Features.Queries.GetCart;
using CartApi.Presentation.Extensions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CartApi.Presentation.Controllers;

[Route("[controller]")]
[ApiController]
public class CartController(IMediator mediator, ILogger<CartController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddCart([FromBody] AddCartCommand command)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid cart");

            await mediator.Send(command);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            logger.LogValidationFailedWhileAddingItemToCartUserId(ex, command.UserId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedErrorWhileAddingItemToCartUserId(ex, command.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest("cartId is required");

            var result = await mediator.Send(new GetCartQuery { UserId = userId });
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogValidationFailedWhileRetrievingCartUserid(ex, userId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedErrorWhileRetrievingCartUserid(ex, userId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveItem([FromQuery] string userId, [FromQuery] string productId)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid cart");

            await mediator.Send(new RemoveItemCommand { UserId = userId, ProductId = productId });

            return Ok();
        }
        catch (ArgumentException ex)
        {
            logger.LogValidationFailedWhileRemovingItemFromCart(ex, userId, productId);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogUnexpectedErrorWhileRemovingItemFromCart(ex, userId, productId);
            return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }
}