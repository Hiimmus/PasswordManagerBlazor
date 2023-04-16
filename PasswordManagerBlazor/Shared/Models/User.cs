
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;


namespace PasswordManagerBlazor.Shared.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        //[Index(IsUnique = true)] //do poprawy 
        public string Email { get; set; }

        public string Password { get; set; }

        public string Hash { get; set; }

        public bool Active { get; set; }

        public ICollection<Role> Roles { get; set; }

        public User() { }

        public User(string firstName, string lastName, string email, string password, bool active)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            Active = active;
        }

        public User(string firstName, string lastName, string email, string password, ICollection<Role> roles)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Password = password;
            Roles = roles;
        }

        public override string ToString()
        {
            return $"User{{ Id={Id}, FirstName='{FirstName}', LastName='{LastName}', Email='{Email}', Password='*********', Roles={Roles} }}";
        }
    }
}
