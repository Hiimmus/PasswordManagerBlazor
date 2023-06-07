using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;
using System.IdentityModel.Tokens.Jwt;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IUserLoginService
    {
        Task<LoginResult> LoginUser(UserLoginDto userLoginDto);
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

        public async Task<LoginResult> LoginUser(UserLoginDto userLoginDto)
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
                    return new LoginResult { Successful = true, Token = _jwtTokenGenerator.GenerateJwtToken(foundUser) };
                }
                else
                {
                    // If the user password was incorrect, return Error = "Password was wrong"
                    return new LoginResult { Successful = false, Error = "Password was wrong" };
                }
            }

            // If the user was not found, return Error = "User was not found"
            return new LoginResult { Successful = false, Error = "User was not found" };
        }
    }
}
