using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IUserRegistrationService
    {
        Task<string> RegisterUser(UserRegistrationDto userDto);
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

        public async Task<string> RegisterUser(UserRegistrationDto userDto)
        {
            // Check if user with the same email already exists
            var existingUser = await _context.Users.SingleOrDefaultAsync(u => u.Email == userDto.Email);
            if (existingUser != null)
            {
                throw new UserAlreadyExistsException("User with the same email already exists.");
            }

            var user = new User
            {
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Hash = _passwordHasher.HashPassword(null, userDto.Password),
                Active = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return _jwtTokenGenerator.GenerateJwtToken(user);
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
