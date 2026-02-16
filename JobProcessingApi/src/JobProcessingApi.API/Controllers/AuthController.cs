using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JobProcessingApi.API.Controllers;

 
//Controller for authentication and token generation
//FOR DEVELOPMENT/TESTING PURPOSES ONLY
  
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

    
    //Generate a JWT token for testing purposes
      
    //<param name="request">Token request with username</param>
    //<returns>JWT token</returns>
    //<response code="200">Token generated successfully</response>
    //<response code="400">Invalid request</response>
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

 
//Request model for token generation
  
public class TokenRequest
{
    
    //Username for the token
      
    public string Username { get; set; } = "test-user";
}

 
//Response model for token generation
  
public class TokenResponse
{
    
    //JWT token string
      
    public string Token { get; set; } = string.Empty;

    
    //Token expiration time
      
    public DateTime ExpiresAt { get; set; }

    
    //Username associated with the token
      
    public string Username { get; set; } = string.Empty;
}
