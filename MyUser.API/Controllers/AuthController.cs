using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyUser.API.Data;
using MyUser.API.Models;
using MyUser.API.Models.Dto;
using MyUser.API.Services;
using Org.BouncyCastle.Crypto.Macs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        


        public AuthController(ApplicationDbContext db, IEmailService emailService, IConfiguration configuration)
        {
            _db = db;
            _emailService = emailService;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
           
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Example: At least 8 characters, one uppercase, one lowercase, one number, and one special character.
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasSymbols = new Regex(@"[@$!%*?&]");

            return hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) && hasLowerChar.IsMatch(password) && hasMiniMaxChars.IsMatch(password) && hasSymbols.IsMatch(password);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null || string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
            {
                return BadRequest("Email and password cannot be null or empty.");
            }

            if (!IsValidEmail(registerDto.Email))
            {
                return BadRequest("Invalid email format.");
            }

            if (!IsValidPassword(registerDto.Password))
            {
                return BadRequest("Password must be 8-15 characters long, contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
            }

            if (await _db.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return BadRequest("Email already exists.");
            }

            using var hmac = new HMACSHA512();
            var user = new User
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Password = registerDto.Password,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Send confirmation email
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", "Please confirm your registration by clicking here.");

            return Ok("User registered successfully. Please check your email to confirm your registration.");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Authenticate([FromBody] LoginDto loginDto)
        {
            //validate user details
            var user = await ValidateUserCredentials(loginDto.Email, loginDto.Password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials.");
            }
            // Retrieve the SecretKey from configuration and check if it's null or empty
            var secretKey = _configuration["Authentication:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("SecretKey", "SecretKey configuration is missing or empty.");
            }

            // Generate token 
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Authentication:SecretKey"]));
            var signingCredentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);
            
            //the claims
            var claimsForToken =new List<Claim>();
            claimsForToken.Add(new Claim("sub", user.Id.ToString()));
            claimsForToken.Add(new Claim("email_address", user.Email));
            claimsForToken.Add(new Claim("given_name", user.FirstName));
            claimsForToken.Add(new Claim("family_name", user.LastName));

            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication: Audience"],
                claimsForToken,
                DateTime.UtcNow,
                DateTime.Now.AddHours(1),
                signingCredentials);
            var tokenToReturn = new JwtSecurityTokenHandler()
                .WriteToken(jwtSecurityToken);

            return Ok(tokenToReturn);
        }

        private async Task<User?> ValidateUserCredentials(string? email, string? password)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                return null;
            }
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i])
                {
                    return null;
                }
            }

            return user;
        }

    }
}
