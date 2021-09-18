using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTestTask.Identity.Models
{
    public class Registration:Auth
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string PasswordConfirmation { get; set; }
        public Registration(string login, string name, string password, string passwordConfirmation) : base(login: login, password: password)
        {
            Name = name;
            PasswordConfirmation = passwordConfirmation;
        }

    }
}
