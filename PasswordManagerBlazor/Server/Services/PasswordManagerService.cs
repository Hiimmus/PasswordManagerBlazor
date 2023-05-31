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
        Task<IEnumerable<PasswordDto>> GetPasswordsForUser(long userId);

        Task RemovePassword(long passwordId);
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
            //ewentualnie wywolane w controlerze i przekazane jako argument to : var userId = User.FindFirst(ClaimTypes.Name)?.Value; 

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

        public async Task<IEnumerable<PasswordDto>> GetPasswordsForUser(long userId)
        {
            var user = await _context.Users.Include(u => u.Passwords).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }

            return user.Passwords.Select(p => new PasswordDto
            {
                Id = ((int)p.Id),
                Url = p.Url,
                Email = p.Email,
                PasswordHash = p.PasswordHash,
                Duplicate = p.Duplicate
            }).ToList();
        }

        public async Task RemovePassword(long passwordId)
        {
            var password = await _context.UserPasswords.FindAsync(passwordId);
            if (password != null)
            {
                _context.UserPasswords.Remove(password);
                await _context.SaveChangesAsync();
            }
        }


    }
}
