using Xunit;
using Microsoft.Extensions.Configuration;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Shared.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class JwtTokenGeneratorTests
{
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var appSettings = new Dictionary<string, string>
        {
            {"Jwt:Key", "thisistestkeythisistestkey123456"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettings)
            .Build();

        _generator = new JwtTokenGenerator(configuration);
    }

    [Fact]
    public async Task GenerateJwtToken_ReturnsExpectedToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Hash = "hashedPassword",
            Active = true
        };

        var appSettingsStub = new Dictionary<string, string> {
        {"Jwt:Key", "verylongsecretkeythatissupersecure"},
        {"Jwt:Issuer", "testIssuer"},
        {"Jwt:Audience", "testAudience"}
    };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettingsStub)
            .Build();

        var jwtTokenGenerator = new JwtTokenGenerator(configuration);

        // Act
        var token = jwtTokenGenerator.GenerateJwtToken(user);

        // Assert
        Assert.NotNull(token);

        // Validate the token
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
        };

        SecurityToken validatedToken;
        var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        var userId = claimsPrincipal?.Identity?.Name;

        Assert.Equal(user.Id.ToString(), userId);
    }


}
