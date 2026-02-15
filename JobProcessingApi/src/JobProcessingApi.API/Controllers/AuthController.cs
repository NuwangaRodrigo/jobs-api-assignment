using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobProcessingApi.API.Controllers;

/// <summary>
/// Controller for authentication and token generation
/// FOR DEVELOPMENT/TESTING PURPOSES ONLY
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Generate a JWT token for testing purposes
    /// </summary>
    /// <param name="request">Token request with username</param>
    /// <returns>JWT token</returns>
    /// <response code="200">Token generated successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult GenerateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest(new { message = "Username is required" });
        }

        _logger.LogInformation("Generating token for user: {Username}", request.Username);

        var key = _configuration["Jwt:Key"] ?? "YourSuperSecretKeyThatShouldBeStoredSecurely12345";
        var issuer = _configuration["Jwt:Issuer"] ?? "JobProcessingApi";
        var audience = _configuration["Jwt:Audience"] ?? "JobProcessingApiClients";
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "60");

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, request.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, request.Username)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new TokenResponse
        {
            Token = tokenString,
            ExpiresAt = token.ValidTo,
            Username = request.Username
        });
    }
}

/// <summary>
/// Request model for token generation
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Username for the token
    /// </summary>
    public string Username { get; set; } = "test-user";
}

/// <summary>
/// Response model for token generation
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// JWT token string
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Username associated with the token
    /// </summary>
    public string Username { get; set; } = string.Empty;
}
