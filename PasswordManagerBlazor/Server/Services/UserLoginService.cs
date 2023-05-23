using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IUserLoginService
    {
        Task<string> LoginUser(UserLoginDto userLoginDto);
    }

    public class UserLoginService : IUserLoginService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserLoginService(ApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> LoginUser(UserLoginDto userLoginDto)
        {
            // Search for the user
            var foundUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userLoginDto.Email);

            if (foundUser != null)
            {
                // Verify the password
                var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(foundUser, foundUser.Hash, userLoginDto.Password);

                if (passwordVerificationResult == PasswordVerificationResult.Success)
                {
                    // Generate and return the JWT token
                    return _jwtTokenGenerator.GenerateJwtToken(foundUser);
                }
            }

            // If the user was not found or password was incorrect, return null
            return null;
        }
    }
}
