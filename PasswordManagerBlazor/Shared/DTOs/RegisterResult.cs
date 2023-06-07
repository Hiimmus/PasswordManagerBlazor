using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PasswordManagerBlazor.Shared.DTOs
{
    public class RegisterResult
    {
        public bool Successful { get; set; }
        public string? Error { get; set; }
    }
}
