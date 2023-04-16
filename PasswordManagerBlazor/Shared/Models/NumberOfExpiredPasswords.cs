
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace PasswordManagerBlazor.Shared.Models
    {
        public class NumberOfExpiredPasswords
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public long Id { get; set; }
            public int Counter { get; set; }
            public DateTime Date { get; set; }
            public long UserId { get; set; }

            public NumberOfExpiredPasswords()
            {
            }

            public NumberOfExpiredPasswords(int counter, long userId)
            {
                Counter = counter;
                Date = DateTime.Now;
                UserId = userId;
            }
        }
    }
