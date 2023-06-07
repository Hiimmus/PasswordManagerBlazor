using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using PasswordManagerBlazor.Shared.Models;

namespace TestProject1
{
    public class UserLoginServiceTest
    {
        private readonly UserLoginService _service;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserLoginServiceTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // InMemory database
                .Options;

            _context = new ApplicationDbContext(options);
            _passwordHasher = new PasswordHasher<User>();
            _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _service = new UserLoginService(_context, _mockJwtTokenGenerator.Object);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnJwtToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Hash = _passwordHasher.HashPassword(null, "Test123!"),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userLoginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "Test123!"
            };

            _mockJwtTokenGenerator
                .Setup(j => j.GenerateJwtToken(It.IsAny<User>()))
                .Returns("test-token"); // Mock JWT token generation

            // Act
            var result = await _service.LoginUser(userLoginDto);

            // Assert
            Assert.Equal(true, result.Successful);
            Assert.Equal("test-token", result.Token); // Verify the JWT token
        }

        [Fact]
        public async Task LoginUser_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var user = new User
            {
                Email = "test2@example.com",
                FirstName = "Test",
                LastName = "User",
                Hash = _passwordHasher.HashPassword(null, "Test123!"),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userLoginDto = new UserLoginDto
            {
                Email = "test@example.com",
                Password = "IncorrectPassword"
            };

            // Act
            var result = await _service.LoginUser(userLoginDto);

            // Assert
            Assert.Equal(false, result.Successful);
            Assert.Equal("Password was wrong", result.Error);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnFalse_WhenLoginIsIncorrect()
        {
            // Arrange
            var user = new User
            {
                Email = "test3@example.com",
                FirstName = "Test",
                LastName = "User",
                Hash = _passwordHasher.HashPassword(null, "Test123!"),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userLoginDto = new UserLoginDto
            {
                Email = "testy@example.com",
                Password = "Test123!"
            };

            // Act
            var result = await _service.LoginUser(userLoginDto);

            // Assert
            Assert.Equal(false, result.Successful);
            Assert.Equal("User was not found", result.Error);
        }
    }
}
