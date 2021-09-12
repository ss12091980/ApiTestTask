using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Auth.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Auth.Identity;
using Auth.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace WebApiEmpty.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IOptions<AuthOptions> authOptions;
        private readonly AppDbContext context;

        public AuthController(IOptions<AuthOptions> authOptions, AppDbContext context)
        {
            this.authOptions = authOptions;
            this.context = context;
        }

        [Route("registration")]
        [HttpPost]
        public IActionResult Registration([FromBody] Registration registration)
        {
            if (registration.Password != registration.PasswordConfirmation)
                return BadRequest(new { errorText = "Password and PasswordConfirmation do not match" });

            if (!ContainsLogin(registration.Login))
            {
                AddNewAccount(registration);
                if (AuthenticateUser(registration.Login, registration.Password))
                {
                    var token = GenerateJWT(login: registration.Login, password: registration.Password);
                    return Ok(new { access_token = token });
                }
                else
                    return Unauthorized();
            }
            else
                return BadRequest(new { errorText = "Login used" });
        }

        private void AddNewAccount(Registration registration)
        {
            context.Users.Add(new User(login: registration.Login, name:registration.Name, password:registration.Password));
            context.SaveChanges();
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login([FromBody] Auth.Identity.Models.Auth login)
        {
            if (AuthenticateUser(login.Login, login.Password))
            {
                var token = GenerateJWT(login: login.Login,password: login.Password);
                return Ok(new { access_token = token });
            }
            else
                return Unauthorized();
        }

        [Route("users")]
        [HttpGet]
        [Authorize]
        public IActionResult GetUsers()
        {
            return Ok(context.Users.Select(user => new { user.Login, user.Name }));
        }

        private bool AuthenticateUser(string login, string password)
        {
            return context.Users.AsQueryable().Count(user => user.Login == login && user.Password == password) == 1;
        }

        private bool ContainsLogin(string login)
        {
            return context.Users.AsQueryable().Count(user => user.Login == login) == 1;
        }

        private string GenerateJWT(string login, string password)
        {
            var authParams = authOptions.Value;

            var securityKey = authParams.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, login),
                new Claim(JwtRegisteredClaimNames.Sub, password)
            };

            var token = new JwtSecurityToken(
                issuer: authParams.Issuer, 
                audience: authParams.Audience, 
                claims: claims, 
                notBefore: DateTime.Now, 
                expires: DateTime.Now.AddSeconds(authParams.TokenLifetime), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
