using AGVSystemCommonNet6.DATABASE;
using AGVSystemCommonNet6.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AGVSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AGVSDbContext _userDbContext;

        public AuthController(AGVSDbContext dbContext)
        {
            _userDbContext = dbContext;
        }

        [HttpGet("Users")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(_userDbContext.Users.ToList());
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserLoginRequest request)
        {
            var existingUser = await _userDbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);
            if (existingUser != null)
            {
                return BadRequest(new { Success = false, Message = "Username already exists" });
            }

            var user = new UserEntity
            {
                UserName = request.Username,
                Password = request.Password
            };
            await _userDbContext.Users.AddAsync(user);
            await _userDbContext.SaveChangesAsync();

            return Ok(new { Success = true });
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);
            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "Invalid username", UserName = "" });
            }

            if (user.Password != request.Password)
            {
                return BadRequest(new { Success = false, Message = "Invalid password", UserName = "" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my_secret_key");
            // TODO: 根據需要生成 JWT Token
            var token = GenerateJwtToken(request.Username, request.Password, user.Role);
            // 返回 JWT Token
            return Ok(new { Success = true, token = token, Role = user.Role, UserName = request.Username });
        }

        [HttpPost("modify")]
        [Authorize]
        public async Task<IActionResult> SaveUserSettings(List<UserEntity> usersData)
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            foreach (var user in usersData)
            {
                var user_find = await _userDbContext.Users.FirstOrDefaultAsync(_user => _user.UserName == user.UserName);

                if (user_find != null)
                {
                    user_find.Password = user.Password;
                    user_find.Role = user.Role;
                    int change_cnt = _userDbContext.SaveChanges();
                }
            }
            return Ok(new { Success = true });
        }

        private string GenerateJwtToken(string username, string password, ERole role)
        {
            // TODO: 根據需要生成 JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("secret_keysecret_keysecret_key11");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim("Password",password),
                    new Claim("Role",role.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }

}
