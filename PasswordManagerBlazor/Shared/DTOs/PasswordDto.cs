
namespace PasswordManagerBlazor.Shared.DTOs
{
    public class PasswordDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime LastChange { get; set; }

        public PasswordDto()
        {
        }

        public PasswordDto(int id, string url, string email, string passwordHash, DateTime lastChange)
        {
            Id = id;
            Url = url;
            Email = email;
            PasswordHash = passwordHash;
            LastChange = lastChange;
        }
    }
}