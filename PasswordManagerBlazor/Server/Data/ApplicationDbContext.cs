using Microsoft.EntityFrameworkCore;
using PasswordManagerBlazor.Shared.Models;

namespace PasswordManagerBlazor.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<EmailDetails> EmailDetails { get; set; }
        public DbSet<PasswordModel> UserPasswords { get; set; }

    }
}
