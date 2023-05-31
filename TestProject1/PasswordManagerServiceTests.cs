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
using static PasswordManagerBlazor.Client.Pages.Duplicates;
using PasswordDto = PasswordManagerBlazor.Shared.DTOs.PasswordDto;

namespace TestProject1
{
    public class PasswordManagerServiceTests
    {
        private PasswordManagerService _passwordManagerService;
        private ApplicationDbContext _context;

        private void Setup(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            _context = new ApplicationDbContext(options);

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
            Setup(Guid.NewGuid().ToString());

            long userId1 = 1;

            var passwordDto = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "hash",
            };

            await _passwordManagerService.AddPassword(passwordDto, userId1);

            var passwordModel = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Email == passwordDto.Email && p.Url == passwordDto.Url);

            Assert.NotNull(passwordModel);
            Assert.Equal(passwordDto.Email, passwordModel.Email);
            Assert.Equal(passwordDto.Url, passwordModel.Url);
            Assert.Equal(passwordDto.PasswordHash, passwordModel.PasswordHash);
        }

        [Fact]
        public async void AddPassword_AddsPasswordCorrectlyWithDuplicates()
        {
            Setup(Guid.NewGuid().ToString());

            long userId1 = 1;

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

            await _passwordManagerService.AddPassword(passwordDto1, userId1);

            var passwordModel1 = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Email == passwordDto1.Email && p.Url == passwordDto1.Url);

            await _passwordManagerService.AddPassword(passwordDto2, userId1);

            var passwordModels = await _context.UserPasswords
                                    .Where(p => p.Email == passwordDto1.Email && p.Url == passwordDto1.Url)
                                    .ToListAsync();

            Assert.NotNull(passwordModels);
            Assert.Equal(2, passwordModels.Count);
            Assert.Equal(passwordDto1.Email, passwordModels[0].Email);
            Assert.Equal(passwordDto1.Url, passwordModels[0].Url);
            Assert.Equal(passwordDto1.PasswordHash, passwordModels[0].PasswordHash);
            Assert.Equal(true, passwordModels[0].Duplicate);

            if (passwordModels.Count > 1)
            {
                Assert.Equal(passwordDto2.Email, passwordModels[1].Email);
                Assert.Equal(passwordDto2.Url, passwordModels[1].Url);
                Assert.Equal(passwordDto2.PasswordHash, passwordModels[1].PasswordHash);
                Assert.Equal(true, passwordModels[1].Duplicate);
            }
        }

        [Fact]
        public async void GetPasswordsForUser_ReturnsCorrectPasswords()
        {
            Setup(Guid.NewGuid().ToString());
            long userId1 = 1;
            long userId2 = 2;

            var user1 = new User { Id = userId1, Email = "user1@example.com", FirstName = "User1", LastName = "Test", Hash = "hash1", Active = true };
            var user2 = new User { Id = userId2, Email = "user2@example.com", FirstName = "User2", LastName = "Test", Hash = "hash2", Active = true };
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            // Check that the users were added correctly.
            var users = await _context.Users.ToListAsync();
            Assert.Equal(2, users.Count);

            var password1 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash1", Url = "url1", UserId = userId1 };
            var password2 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash2", Url = "url2", UserId = userId1 };
            var password3 = new PasswordModel { Email = "user2@example.com", PasswordHash = "hash3", Url = "url3", UserId = userId2 };

            await _context.UserPasswords.AddRangeAsync(password1, password2, password3);
            await _context.SaveChangesAsync();

            // Check that the passwords were added correctly.
            var passwords = await _context.UserPasswords.ToListAsync();
            Assert.Equal(3, passwords.Count);

            var result = await _passwordManagerService.GetPasswordsForUser(userId1);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async void DeletePasswordForUsers()
        {
            //GIVEN
            Setup(Guid.NewGuid().ToString());
            long userId1 = 1;
            long userId2 = 2;

            //AND: TWO USERS
            var user1 = new User { Id = userId1, Email = "user1@example.com", FirstName = "User1", LastName = "Test", Hash = "hash1", Active = true };
            var user2 = new User { Id = userId2, Email = "user2@example.com", FirstName = "User2", LastName = "Test", Hash = "hash2", Active = true };
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            // Check that the users were added correctly.
            var users = await _context.Users.ToListAsync();
            Assert.Equal(2, users.Count);

            //AND: THREE PASSWORDS
            var password1 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash1", Url = "url1", UserId = userId1 };
            var password2 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash2", Url = "url2", UserId = userId1 };
            var password3 = new PasswordModel { Email = "user2@example.com", PasswordHash = "hash3", Url = "url3", UserId = userId2 };

            await _context.UserPasswords.AddRangeAsync(password1, password2, password3);
            await _context.SaveChangesAsync();

            // Check that the passwords were added correctly.
            var passwords = await _context.UserPasswords.ToListAsync();
            Assert.Equal(3, passwords.Count);

            //WHEN
             await _passwordManagerService.RemovePassword((int)passwords[0].Id);

            //THEN
            var passwords_after_delete = await _context.UserPasswords.ToListAsync();
            Assert.Equal(2, passwords_after_delete.Count);
        }

        [Fact]
        public async void UpdatePassword_UpdatesPasswordCorrectly()
        {
            // Arrange
            Setup(Guid.NewGuid().ToString());
            long userId1 = 1;
            var passwordDto = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "hash",
            };

            await _passwordManagerService.AddPassword(passwordDto, userId1);

            var passwordModel = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Email == passwordDto.Email && p.Url == passwordDto.Url);

            Assert.NotNull(passwordModel);
            Assert.Equal(passwordDto.Email, passwordModel.Email);
            Assert.Equal(passwordDto.Url, passwordModel.Url);
            Assert.Equal(passwordDto.PasswordHash, passwordModel.PasswordHash);

            // Prepare for update
            passwordDto = await _passwordManagerService.GetPassword((int)passwordModel.Id);

            passwordDto.Url = "https://updatedexample.com";
            passwordDto.Email = "updatedtest@example.com";
            passwordDto.PasswordHash = "updatedhash";

            // Act
            await _passwordManagerService.UpdatePassword(passwordDto);

            // Assert
            var updatedPasswordModel = await _context.UserPasswords.FirstOrDefaultAsync(p => p.Id == passwordModel.Id);

            Assert.NotNull(updatedPasswordModel);
            Assert.Equal(passwordDto.Email, updatedPasswordModel.Email);
            Assert.Equal(passwordDto.Url, updatedPasswordModel.Url);
            Assert.Equal(passwordDto.PasswordHash, updatedPasswordModel.PasswordHash);
            Assert.True(updatedPasswordModel.LastChange.Date == DateTime.UtcNow.Date);
        }

        [Fact]
        public async void GetDuplicatePasswordsForUser_ReturnsCorrectDuplicates()
        {

            // Arrange
            Setup(Guid.NewGuid().ToString());


            long userId1 = 1;
            long userId2 = 2;

            //AND: TWO USERS
            var user1 = new User { Id = userId1, Email = "user1@example.com", FirstName = "User1", LastName = "Test", Hash = "hash1", Active = true };
            var user2 = new User { Id = userId2, Email = "user2@example.com", FirstName = "User2", LastName = "Test", Hash = "hash2", Active = true };
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

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

            var passwordDto3 = new PasswordDto
            {
                Url = "https://example.com",
                Email = "test@example.com",
                PasswordHash = "nowyhash",
            };

            await _passwordManagerService.AddPassword(passwordDto1, userId1);
            await _passwordManagerService.AddPassword(passwordDto2, userId1);
            await _passwordManagerService.AddPassword(passwordDto3, userId2);


            var passwordModels = await _context.UserPasswords
                        .Where(p => p.Email == passwordDto1.Email && p.Url == passwordDto1.Url)
                        .ToListAsync();

            Assert.NotNull(passwordModels);
            Assert.Equal(3, passwordModels.Count);
            Assert.Equal(passwordDto1.Email, passwordModels[0].Email);
            Assert.Equal(passwordDto1.Url, passwordModels[0].Url);
            Assert.Equal(passwordDto1.PasswordHash, passwordModels[0].PasswordHash);
            Assert.Equal(true, passwordModels[0].Duplicate);
            if (passwordModels.Count > 1)
            {
                Assert.Equal(passwordDto2.Email, passwordModels[1].Email);
                Assert.Equal(passwordDto2.Url, passwordModels[1].Url);
                Assert.Equal(passwordDto2.PasswordHash, passwordModels[1].PasswordHash);
                Assert.Equal(true, passwordModels[1].Duplicate);
            }

            // Act
            var result = await _passwordManagerService.GetDuplicatePasswordsForUser(userId1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            var duplicatePassword = result.First();
            Assert.True(duplicatePassword.Duplicate);
        }

        [Fact]
        public async void GetExpieredPasswordsForUser_ReturnsCorrectExpieredPasswords()
        {
            //GIVEN
            Setup(Guid.NewGuid().ToString());
            long userId1 = 1;
            long userId2 = 2;

            //AND: TWO USERS
            var user1 = new User { Id = userId1, Email = "user1@example.com", FirstName = "User1", LastName = "Test", Hash = "hash1", Active = true };
            var user2 = new User { Id = userId2, Email = "user2@example.com", FirstName = "User2", LastName = "Test", Hash = "hash2", Active = true };
            await _context.Users.AddRangeAsync(user1, user2);
            await _context.SaveChangesAsync();

            // Check that the users were added correctly.
            var users = await _context.Users.ToListAsync();
            Assert.Equal(2, users.Count);

            //AND: THREE PASSWORDS
            var password1 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash1", Url = "url1", UserId = userId1, LastChange = DateTime.Now.AddMonths(-2) };
            var password2 = new PasswordModel { Email = "user1@example.com", PasswordHash = "hash2", Url = "url2", UserId = userId1, LastChange = DateTime.Now };
            var password3 = new PasswordModel { Email = "user2@example.com", PasswordHash = "hash3", Url = "url3", UserId = userId2, LastChange = DateTime.Now.AddMonths(-2) };

            await _context.UserPasswords.AddRangeAsync(password1, password2, password3);
            await _context.SaveChangesAsync();

            // Check that the passwords were added correctly.
            var passwords = await _context.UserPasswords.ToListAsync();
            Assert.Equal(3, passwords.Count);

            //WHEN
            var result = await _passwordManagerService.GetExpiredPasswordsForUser(userId1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Count());
        }


    }
}
