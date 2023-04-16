using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

    namespace PasswordManagerBlazor.Shared.Models
{
        public class PasswordModel
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }

            [Required]
            public string Email { get; set; }

            [Required]
            public string PasswordHash { get; set; }

            [Required]
            public string Url { get; set; }

            public DateTime LastChange { get; set; }

            public bool Duplicate { get; set; }

            [ForeignKey("UserId")]
            public User User { get; set; }

            public int UserId { get; set; }

            public PasswordModel() { }

            public PasswordModel(string email, string passwordHash, string url, DateTime lastChange)
            {
                Email = email;
                PasswordHash = passwordHash;
                Url = url;
                LastChange = lastChange;
            }

            public override string ToString()
            {
                return $"Password{{ Id={Id}, Email='{Email}', PasswordHash='{PasswordHash}', LastChange='{LastChange}' }}";
            }
        }
    }
