using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Identity.Models
{
    public class Auth
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }

        public Auth(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}
