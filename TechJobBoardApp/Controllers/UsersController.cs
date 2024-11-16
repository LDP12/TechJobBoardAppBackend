using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using FreelanceJobBoard.Models;
using FreelanceJobBoard.Services;
using System.Security.Cryptography;
using System.Text;
using FreelanceJobBoard.Data;
using FreelanceJobBoard.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace FreelanceJobBoard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _Context;
        private readonly TokenService _TokenService;

        public UsersController(AppDbContext context, TokenService tokenService)
        {
            _Context = context;
            _TokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegistrationDto dto)
        {
            if (_Context.Users.Any(u => u.Email == dto.Email))
                return BadRequest("User with this email already exists.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role,
                IsPremium = false
            };

            _Context.Users.Add(user);
            await _Context.SaveChangesAsync();

            var token = _TokenService.GenerateToken(user.UserId, user.Role.ToString());
            return Ok(new { Token = token, Role = user.Role.ToString() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto dto)
        {
            var user = _Context.Users.SingleOrDefault(u => u.Email == dto.Email);
            if (user == null || !VerifyPasswordHash(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var token = _TokenService.GenerateToken(user.UserId, user.Role.ToString());
            return Ok(new { Token = token, Role = user.Role.ToString() });
        }
        
        [HttpPost("boost-profile")]
        [Authorize(Roles = "Freelancer")]
        public async Task<IActionResult> BoostProfile()
        {
            var userId = int.Parse(User.Identity.Name);

            var user = await _Context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            user.IsBoosted = true;
            await _Context.SaveChangesAsync();

            return Ok(new { Message = "Profile boosted successfully." });
        }
        
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            var hash = HashPassword(password);
            return hash == storedHash;
        }
    }
}