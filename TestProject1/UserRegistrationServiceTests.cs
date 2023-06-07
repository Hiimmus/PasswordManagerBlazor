using Moq;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.Models;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Shared.DTOs;


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
                Email = "test1@example.com",
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
            Assert.Equal(true, result.Successful); // Verify the JWT token
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            Assert.NotNull(user); // Verify the user was created
            Assert.Equal(userDto.FirstName, user.FirstName);
            Assert.Equal(userDto.LastName, user.LastName);
            Assert.Equal(userDto.Email, user.Email);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnError_WhenEmailAlreadyExists()
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

            // Act
            var result = await _service.RegisterUser(newUserDto);

            // Assert
            Assert.Equal(false, result.Successful); // Verify the JWT token
            Assert.Equal("User with this email is already registered.", result.Error); 
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnError_WhenPasswordIsEmpty()
        {
            // Arrange
            var userDto = new UserRegistrationDto
            {
                Email = "test2@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "" // Empty password
            };

            // Act
            var result = await _service.RegisterUser(userDto);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal("Password must be at least 3 characters long.", result.Error);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnError_WhenPasswordIsTooShort()
        {
            // Arrange
            var userDto = new UserRegistrationDto
            {
                Email = "test10@example.com",
                FirstName = "Test",
                LastName = "User",
                Password = "12" // Password is too short
            };

            // Act
            var result = await _service.RegisterUser(userDto);

            // Assert
            Assert.False(result.Successful);
            Assert.Equal("Password must be at least 3 characters long.", result.Error);
        }
    }
}


