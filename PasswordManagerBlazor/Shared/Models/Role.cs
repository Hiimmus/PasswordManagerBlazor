
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

   namespace PasswordManagerBlazor.Shared.Models
{
        [Table("Role")]
        public class Role
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }
            public string Name { get; set; }

            public Role() { }

            public Role(string name)
            {
                Name = name;
            }

            public override string ToString()
            {
                return $"Role{{ Id={Id}, Name='{Name}' }}";
            }
        }
    }
