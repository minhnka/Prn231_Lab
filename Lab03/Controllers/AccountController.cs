using BusinessObjects;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Repositories;
using Service;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Lab03.Controllers
{
    [ApiController]
    
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        public AccountController(IAccountService accountService, IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;
        }
        [HttpPost("User/GenerateToken")]

        public IActionResult Login([FromBody] LoginRequest request)
        {
            var account = _accountService.GetByEmailandPassword(request.Email, request.Password);

            if (account == null)
            {
                return Unauthorized(new
                {
                    Success = false,
                    Message = "Invalid email or password.",
                    Data = (object)null
                });
            }

            // Generate JWT token
            var token = GenerateJwtToken(account);

            // Don't return password in response
            account.Password = null;

            return Ok(new
            {
                Success = true,
                Role= account.RoleId.HasValue ? account.RoleId.Value.ToString() : "0", // Default to "0" if no role
                Message = "Login successful",
                Token = token,
            });
        }

        private string GenerateJwtToken(Account user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.AccountName),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("AccountId", user.AccountId.ToString())
    };

            // Add role claim if role property exists
            if (user.RoleId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.RoleId.Value.ToString()));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        [HttpPost("User/CreateAccount")]
        public IActionResult CreateAccount([FromBody] RegisterRequest request)
        {
            try
            {
                // Check if Email is already taken
                var allAccounts = _accountService.GetAll();
                var accountByEmail = allAccounts.FirstOrDefault(a => a.Email == request.Email);
                if (accountByEmail != null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Email is already registered."
                    });
                }

                var account = new Account
                {
                    AccountName = request.AccountName,
                    Email = request.Email,
                    Password = request.Password,
                    RoleId = 2 // Default role: User
                };

                _accountService.Add(account);

                return Ok(new
                {
                    Success = true,
                    Message = "Account created successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while creating the account.",
                    Error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }




        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }


        public class RegisterRequest
        {

            public string AccountName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }
    }
}
