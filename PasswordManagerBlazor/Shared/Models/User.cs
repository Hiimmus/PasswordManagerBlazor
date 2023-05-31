
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
        public string Email { get; set; }
        public string Hash { get; set; }
        public bool Active { get; set; }

        public ICollection<Role> Roles { get; set; }

        public User() { }

       

        public override string ToString()
        {
            return $"User{{ Id={Id}, FirstName='{FirstName}', LastName='{LastName}', Email='{Email}', Password='*********', Roles={Roles} }}";
        }

    }
}
