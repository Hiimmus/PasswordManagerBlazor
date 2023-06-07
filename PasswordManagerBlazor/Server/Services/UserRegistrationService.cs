using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IUserRegistrationService
    {
        Task<RegisterResult> RegisterUser(UserRegistrationDto userDto);
    }

    public class UserRegistrationService : IUserRegistrationService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserRegistrationService(ApplicationDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<RegisterResult> RegisterUser(UserRegistrationDto userDto)
        {
            // Check if user with the same email already exists
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser != null)
            {

                return new RegisterResult { Successful = false, Error = "User with this email is already registered." };
            
            }

            if (string.IsNullOrEmpty(userDto.Password) || userDto.Password.Length < 3)
            {
                return new RegisterResult { Successful = false, Error = "Password must be at least 3 characters long." };
            }

            var user = new User
            {
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Hash = _passwordHasher.HashPassword(null, userDto.Password),
                Active = true
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new RegisterResult { Successful = false, Error = $"An error occurred while registering the user: {ex.Message}" };
            }

            return new RegisterResult { Successful = true };
        }
    }
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException() { }

        public UserAlreadyExistsException(string message)
            : base(message) { }

        public UserAlreadyExistsException(string message, Exception inner)
            : base(message, inner) { }
    }
}
