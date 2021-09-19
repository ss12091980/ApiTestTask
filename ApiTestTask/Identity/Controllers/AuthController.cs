using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTestTask.Identity.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using ApiTestTask.Identity;
using ApiTestTask.Data.Models;
using Microsoft.AspNetCore.Authorization;


namespace ApiTestTask.Identity.Controllers
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
        public async Task<IActionResult> Registration([FromBody] Registration registration)
        {
            if (registration.Password != registration.PasswordConfirmation)
                return BadRequest(new { errorText = "Password and PasswordConfirmation do not match" });

            if (await ContainsLoginAsync(registration.Login) == false)
            {
                await AddNewAccountAsync(registration);
                if (await AuthenticateUserAsync(registration.Login, registration.Password))
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

        private async Task AddNewAccountAsync(Registration registration)
        {
            context.Users.Add(new User(login: registration.Login, name:registration.Name, password:registration.Password));
            await context.SaveChangesAsync();
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> LoginAsync([FromBody] ApiTestTask.Identity.Models.Auth login)
        {
            if (await AuthenticateUserAsync(login.Login, login.Password))
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
        public async Task<IActionResult> GetUsersAsync()
        {
            return Ok(await context.Users.Select(user => new { user.Login, user.Name }).ToListAsync());
        }

        private async Task<bool> AuthenticateUserAsync(string login, string password)
        {
            return await context.Users.AsQueryable().CountAsync(user => user.Login == login && user.Password == password) == 1;
        }

        private async Task<bool> ContainsLoginAsync(string login)
        {
            return await context.Users.AsQueryable().CountAsync(user => user.Login == login) == 1;
        }

        private string GenerateJWT(string login, string password)
        {
            var authParams = authOptions.Value;

            var securityKey = authParams.GetSymmetricSecurityKey();
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, login),
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