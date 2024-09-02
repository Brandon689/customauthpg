using customauthpg.Models;
using Microsoft.Extensions.Options;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace customauthpg.Tests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly JwtSettings _jwtSettings;

    public JwtServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Key = "your-256-bit-secret-key-here-make-it-long-enough",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60
        };

        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(x => x.Value).Returns(_jwtSettings);

        _jwtService = new JwtService(mockOptions.Object);
    }

    [Fact]
    public void ValidateToken_ShouldReturnClaimsPrincipal_WhenTokenIsValid()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "testuser@example.com",
            Role = "User"
        };

        // Act
        var token = _jwtService.GenerateToken(user);
        Console.WriteLine($"Generated Token: {token}");

        var principal = _jwtService.ValidateToken(token);

        // Debug: Print all claims
        if (principal != null)
        {
            foreach (var claim in principal.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
        }
        else
        {
            Console.WriteLine("Principal is null");
        }

        // Assert
        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Equal(user.Username, principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal(user.Email, principal.FindFirst(ClaimTypes.Email)?.Value);
        Assert.Equal(user.Role, principal.FindFirst(ClaimTypes.Role)?.Value);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "testuser@example.com",
            Role = "User"
        };

        // Act
        var token = _jwtService.GenerateToken(user);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
        Assert.Equal(user.Username, jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value);
        Assert.Equal(user.Email, jwtToken.Claims.First(c => c.Type == ClaimTypes.Email).Value);
        Assert.Equal(user.Role, jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value);
    }

    [Fact]
    public void ValidateToken_ShouldReturnNull_WhenTokenIsInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var principal = _jwtService.ValidateToken(invalidToken);

        // Assert
        Assert.Null(principal);
    }
}
