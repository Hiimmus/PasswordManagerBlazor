using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Server.Services;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;
using System;
using System.Security.Claims;
using Xunit;

namespace TestProject1
{
    public class PasswordManagerServiceTests
    {
        private PasswordManagerService _passwordManagerService;
        private ApplicationDbContext _context;

        public PasswordManagerServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);

            // TODO: Set up a mock IHttpContextAccessor that returns a ClaimsPrincipal with a NameIdentifier claim.
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            }));

            mockHttpContextAccessor.Setup(m => m.HttpContext.User).Returns(user);
            _passwordManagerService = new PasswordManagerService(_context, mockHttpContextAccessor.Object);
        }

        [Fact]
        public async void AddPassword_AddsPasswordCorrectly()
        {
            _context.UserPasswords.RemoveRange(_context.UserPasswords);
            _context.SaveChanges();
            var passwordDto = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "hash",
            };

            await _passwordManagerService.AddPassword(passwordDto);

            var passwordModel = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Email == passwordDto.Email && p.Url == passwordDto.Url);

            Assert.NotNull(passwordModel);
            Assert.Equal(passwordDto.Email, passwordModel.Email);
            Assert.Equal(passwordDto.Url, passwordModel.Url);
            Assert.Equal(passwordDto.PasswordHash, passwordModel.PasswordHash);
        }

        [Fact]
        public async void AddPassword_AddsPasswordCorrectlyWithDuplicates()
        {
            _context.UserPasswords.RemoveRange(_context.UserPasswords);
            _context.SaveChanges();

            //GIVEN
            var passwordDto1 = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "hash",
            };

            var passwordDto2 = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "nowyhash",
            };

            //WHEN
            await _passwordManagerService.AddPassword(passwordDto1);

            var passwordModel1 = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Email == passwordDto1.Email && p.Url == passwordDto1.Url);

            await _passwordManagerService.AddPassword(passwordDto2);

            var passwordModels = await _context.UserPasswords
                                    .Where(p => p.Email == passwordDto1.Email && p.Url == passwordDto1.Url)
                                    .ToListAsync();

            Assert.NotNull(passwordModels);

            // Sprawdź czy obiekt passwordModel1 ma właściwe wartości
            Assert.Equal(2, passwordModels.Count);
            Assert.Equal(passwordDto1.Email, passwordModels[0].Email);
            Assert.Equal(passwordDto1.Url, passwordModels[0].Url);
            Assert.Equal(passwordDto1.PasswordHash, passwordModels[0].PasswordHash);
            Assert.Equal(true, passwordModels[0].Duplicate);

            // Sprawdź czy obiekt passwordModel2 ma właściwe wartości
            if (passwordModels.Count > 1)
            {
                Assert.Equal(passwordDto2.Email, passwordModels[1].Email);
                Assert.Equal(passwordDto2.Url, passwordModels[1].Url);
                Assert.Equal(passwordDto2.PasswordHash, passwordModels[1].PasswordHash);
                Assert.Equal(true, passwordModels[1].Duplicate);
            }
        }
    }
}
