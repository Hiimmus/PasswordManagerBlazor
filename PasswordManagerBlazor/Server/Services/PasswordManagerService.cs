using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;
using System.Security.Claims;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IPasswordManagerService
    {
        Task AddPassword(PasswordDto passworddto);
    }

    public class PasswordManagerService : IPasswordManagerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PasswordManagerService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task AddPassword(PasswordDto passwordDto)
        {
            var userId = int.Parse(_httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            var existingPassword = await _context.UserPasswords
                .Where(p => p.UserId == userId && p.Email == passwordDto.Email && p.Url == passwordDto.Url)
                .FirstOrDefaultAsync();

            if (existingPassword != null)
            {
                existingPassword.Duplicate = true;
            }

            var passwordModel = new PasswordModel
            {
                Email = passwordDto.Email,
                PasswordHash = passwordDto.PasswordHash,
                Url = passwordDto.Url,
                Duplicate = existingPassword != null,
                LastChange = DateTime.UtcNow,
                UserId = userId
            };

            _context.UserPasswords.Add(passwordModel);
            await _context.SaveChangesAsync();
        }
    }
}
