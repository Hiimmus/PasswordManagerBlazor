using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Server.Data;
using PasswordManagerBlazor.Shared.DTOs;
using PasswordManagerBlazor.Shared.Models;
using System.Security.Claims;
using static PasswordManagerBlazor.Client.Pages.Duplicates;
using PasswordDto = PasswordManagerBlazor.Shared.DTOs.PasswordDto;

namespace PasswordManagerBlazor.Server.Services
{
    public interface IPasswordManagerService
    {
        Task AddPassword(PasswordDto passworddto, long userId);
        Task<IEnumerable<PasswordDto>> GetPasswordsForUser(long userId);
        Task RemovePassword(int passwordId);
        Task UpdatePassword(PasswordDto passwordDto);
        Task<PasswordDto> GetPassword(int id);
        Task<IEnumerable<PasswordDto>> GetDuplicatePasswordsForUser(long userId);
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

        public async Task AddPassword(PasswordDto passwordDto, long userId)
        {

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

        public async Task RemovePassword(int passwordId)
        {
            long id = passwordId;
            var password = await _context.UserPasswords.FindAsync(id);
            if (password != null)
            {
                _context.UserPasswords.Remove(password);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdatePassword(PasswordDto passwordDto)
        {
            long id = passwordDto.Id;
            var password = await _context.UserPasswords.FindAsync(id);
            if (password != null)
            {
                password.Email = passwordDto.Email;
                password.PasswordHash = passwordDto.PasswordHash;
                password.Url = passwordDto.Url;
                password.LastChange = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<PasswordDto> GetPassword(int id)
        {
            var password = await _context.UserPasswords.FindAsync((long)id);

            if (password == null)
            {
                return null;
            }

            return new PasswordDto
            {
                Id = ((int)password.Id),
                Url = password.Url,
                Email = password.Email,
                PasswordHash = password.PasswordHash,
                Duplicate = password.Duplicate
            };
        }

        public async Task<IEnumerable<PasswordDto>> GetDuplicatePasswordsForUser(long userId)
        {
            var user = await _context.Users.Include(u => u.Passwords).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }
            var userDuplicatePasswords = _context.Users
                .Where(u => u.Id == userId) // Znajduje użytkownika o danym Id.
                .SelectMany(u => u.Passwords);

            var result = userDuplicatePasswords.Where(p => p.Duplicate == true).ToList(); // Filtruje tylko hasła zduplikowane.

            var finalResult = result.Select(p => new PasswordDto
            {
                Id = ((int)p.Id),
                Url = p.Url,
                Email = p.Email,
                PasswordHash = p.PasswordHash,
                Duplicate = p.Duplicate
            }).ToList();

            return finalResult;
        }


        public async Task<IEnumerable<PasswordDto>> GetExpiredPasswordsForUser(long userId)
        {
            var user = await _context.Users.Include(u => u.Passwords).FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return null;
            }
            var userDuplicatePasswords = _context.Users
                .Where(u => u.Id == userId) // Znajduje użytkownika o danym Id.
                .SelectMany(u => u.Passwords);

            var result = userDuplicatePasswords.Where(p => p.LastChange < DateTime.Now.AddMonths(-1)).ToList();

            var finalResult = result.Select(p => new PasswordDto
            {
                Id = ((int)p.Id),
                Url = p.Url,
                Email = p.Email,
                PasswordHash = p.PasswordHash,
                Duplicate = p.Duplicate
            }).ToList();

            return finalResult;
        }
    }
}
