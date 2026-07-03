using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserApi.Application.Features.Commands.AccountVerification;
using UserApi.Application.Features.Commands.Delete;
using UserApi.Application.Features.Commands.SignUp;
using UserApi.Application.Features.Queries.GetCurrentUser;
using UserApi.Application.Features.Queries.SignIn;
using UserApi.Application.Services;

namespace UserApi.Presentation.Controllers;

/// <summary>
/// User account endpoints for registration, authentication, and profile access.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController(IMediator mediator, ILogger<UserController> logger) : ControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="signUpCommand">User registration payload.</param>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">The request payload is invalid.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [Route("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignUpCommand signUpCommand)
    {
        Logs.LogCalledSignupNameByUnknown(logger, nameof(SignUp), User.FindFirst("appid")?.Value ?? "Unknown");
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await mediator.Send(signUpCommand);
            if (result)
            {
                Logs.LogSuccessfullyRegisteredNewUserEmailCalledByUnknown(logger, signUpCommand.Email,
                    User.FindFirst("appid")?.Value ?? "Unknown");
                return CreatedAtAction(nameof(SignIn), new { email = signUpCommand.Email });
            }
        }
        catch (Exception e)
        {
            logger.LogAnErrorOccurredWhileSigningUp(e);
            return StatusCode(500, "An error occurred while signing up.");
        }

        Logs.LogCouldNotRegisterUserEmailCalledByUnknown(logger, signUpCommand.Email,
            User.FindFirst("appid")?.Value ?? "Unknown");
        return BadRequest();
    }

    /// <summary>
    /// Authenticates a user and issues an access token cookie.
    /// </summary>
    /// <param name="signInQuery">User credentials.</param>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">Email or password is missing.</response>
    /// <response code="401">Authentication failed.</response>
    [HttpPost]
    [Route("signin")]
    public async Task<IActionResult> SignIn([FromBody] SignInQuery signInQuery)
    {
        Logs.LogCalledSignupNameByUnknown(logger, nameof(SignUp), User.FindFirst("appid")?.Value ?? "Unknown");

        if (!ModelState.IsValid)
        {
            Logs.LogCouldNotSignInBecauseEmailOrPasswordIsEmpty(logger);
            return BadRequest("Email or password is empty.");
        }

        var token = await mediator.Send(signInQuery);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized();
        }

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = TimeSpan.FromHours(1)
        });

        Logs.LogSuccessfullySignedInUserEmailCalledSignupNameByUnknown(logger, signInQuery.Email, nameof(SignUp),
            User.FindFirst("appid")?.Value ?? "Unknown");
        return Ok();
    }

    /// <summary>
    /// Gets the current authenticated user profile.
    /// </summary>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="401">The request is not authenticated.</response>
    [HttpGet]
    [Route("me")]
    [Authorize(AuthenticationSchemes = "CookieJwt")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized();

        var user = await mediator.Send(new GetCurrentUserQuery { Email = email });
        if (user == null)
            return Unauthorized();

        return Ok(user);
    }

    /// <summary>
    /// Signs out the current user by clearing the access token cookie.
    /// </summary>
    /// <response code="200">Sign-out succeeded.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [Route("signout")]
    [Authorize(AuthenticationSchemes = "CookieJwt")]
    public new Task<IActionResult> SignOut()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "unknown";
        Logs.LogSignoutRequestReceivedUserEmail(logger, email);

        try
        {

            Logs.LogSuccessfullySignedOutUserEmail(logger, email);

            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });

            return Task.FromResult<IActionResult>(Ok());
        }
        catch (Exception ex)
        {
            Logs.LogAnErrorOccurredWhileSigningOutUserEmail(logger, ex, email);
            return Task.FromResult<IActionResult>(StatusCode(500, "Internal server error during sign-out."));
        }
    }

    /// <summary>
    /// Verifies a user's account using the activation key.
    /// </summary>
    /// <param name="accountVerificationCommand">Account verification payload.</param>
    /// <response code="200">Verification succeeded.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [Route("accountverification")]
    public async Task<IActionResult> AccountVerification(
        [FromBody] AccountVerificationCommand accountVerificationCommand)
    {
        try
        {
            var result = await mediator.Send(accountVerificationCommand);

            Logs.LogAccountVerificationSuccessfulForAccountAccount(logger, accountVerificationCommand.Email);

            return Ok(result);
        }
        catch (Exception ex)
        {
            Logs.LogErrorOccurredDuringVerificationForAccountAccount(logger, ex, accountVerificationCommand.Email);
            return StatusCode(500, "An error occurred while verifying the account.");
        }
    }

    /// <summary>
    /// Deletes a user account by email.
    /// </summary>
    /// <param name="email">The account email to delete.</param>
    /// <response code="204">Account deleted.</response>
    /// <response code="400">Email is missing.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete]
    [Route("delete")]
    public async Task<IActionResult> Delete([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email cannot be null or empty.");

        try
        {
            await mediator.Send(new DeleteCommand { Email = email });
            return NoContent();
        }
        catch
        {
            return StatusCode(500, "An error occurred while deleting the account.");
        }
    }
}
