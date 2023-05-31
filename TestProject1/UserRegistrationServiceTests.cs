using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Shared.DTOs;
using Microsoft.Extensions.Configuration;
using Castle.Components.DictionaryAdapter.Xml;

namespace TestProject1
{
    public class UserRegistrationServiceTests
    {
        private readonly UserRegistrationService _service;
        private readonly Mock<IJwtTokenGenerator> _mockJwtTokenGenerator;
        private readonly ApplicationDbContext _context;


        public UserRegistrationServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase") // InMemory database
                .Options;

            _context = new ApplicationDbContext(options);
            _mockJwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _service = new UserRegistrationService(_context, _mockJwtTokenGenerator.Object);
        }

        [Fact]
        public async Task RegisterUser_ShouldCreateUserAndReturnJwtToken()
        {
            _context.UserPasswords.RemoveRange(_context.UserPasswords);
            _context.Users.RemoveRange(_context.Users);
            _context.SaveChanges();

            // Arrange
            var userDto = new UserRegistrationDto
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "Test123!"
            };

            _mockJwtTokenGenerator
                .Setup(j => j.GenerateJwtToken(It.IsAny<User>()))
                .Returns("test-token"); // Mock JWT token generation

            // Act
            var result = await _service.RegisterUser(userDto);

            // Assert
            Assert.Equal("test-token", result); // Verify the JWT token
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            Assert.NotNull(user); // Verify the user was created
            Assert.Equal(userDto.FirstName, user.FirstName);
            Assert.Equal(userDto.LastName, user.LastName);
            Assert.Equal(userDto.Email, user.Email);
        }

        [Fact]
        public async Task RegisterUser_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var existingUser = new User
            {
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                Hash = "ExistingHash",
                Active = true
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var newUserDto = new UserRegistrationDto
            {
                Email = "existing@example.com", // The same email as the existing user
                FirstName = "New",
                LastName = "User",
                Password = "New123!"
            };

            // Act and Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => _service.RegisterUser(newUserDto));
            Assert.Equal("User with the same email already exists.", ex.Message);
        }





    }

}


