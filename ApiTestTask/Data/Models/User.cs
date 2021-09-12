using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Auth.Data.Models
{
    public class User
    {
        public User(string name, string login, string password)
        {
            Name = name;
            Login = login;
            Password = password;
        }
        [Key]
        public string Login { get; set; }
        public string Name { get; set; }

        public string Password { get; set; }
    }
}
