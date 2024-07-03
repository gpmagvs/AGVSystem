using AGVSystem.Service;
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
        private readonly UserValidationService userValidationService;

        public AuthController(AGVSDbContext dbContext, UserValidationService userValidationService)
        {
            _userDbContext = dbContext;
            this.userValidationService = userValidationService;
        }
        [HttpPost("Verify")]
        [Authorize]
        public async Task<IActionResult> VerifyUser()
        {
            if (userValidationService.UserValidation(HttpContext))
            {
                return Ok(new { Success = true });
            }
            else
            {
                return Ok(new { Success = false });
            }
        }
        /// <summary>
        /// 取得用戶列表
        /// </summary>
        /// <returns>用戶列表</returns>
        [HttpGet("Users")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            return Ok(_userDbContext.Users.ToList());
        }

        /// <summary>
        /// 註冊新用戶
        /// </summary>
        /// <param name="request">用戶註冊請求</param>
        /// <returns>註冊結果</returns>
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

        /// <summary>
        /// 用戶登入
        /// </summary>
        /// <param name="request">用戶登入請求</param>
        /// <returns>登入結果及 JWT Token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginRequest request)
        {
            HttpContext.Request.Headers.TryGetValue("Authorization", out var token);
            var user = await _userDbContext.Users.FirstOrDefaultAsync(u => u.UserName == request.Username);
            if (user == null || user.Password != request.Password)
            {
                return BadRequest(new { Success = false, Message = "Invalid User Name or Password", UserName = "" });
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("my_secret_key");
            // TODO: 根據需要生成 JWT Token
            token = GenerateJwtToken(request.Username, request.Password, user.Role);
            // 返回 JWT Token
            return Ok(new { Success = true, token = token, Role = user.Role, UserName = request.Username });
        }
        /// <summary>
        /// 修改用戶設定
        /// </summary>
        /// <param name="usersData">用戶資料</param>
        /// <returns>修改結果</returns>
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
                else
                {
                    _userDbContext.Add(user);
                }
            }
            _userDbContext.SaveChanges();
            return Ok(new { Success = true });
        }

        /// <summary>
        /// 刪除用戶
        /// </summary>
        /// <param name="user_name">用戶名稱</param>
        /// <returns>刪除結果</returns>
        [HttpDelete("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string user_name)
        {
            var user_ = _userDbContext.Users.FirstOrDefault(user => user.UserName == user_name);
            if (user_ != null)
            {
                _userDbContext.Users.Remove(user_);
                _userDbContext.SaveChanges();
            }
            return Ok(new { Success = true });
        }

        /// <summary>
        /// 新增用戶
        /// </summary>
        /// <param name="new_user">新用戶</param>
        /// <returns>新增結果</returns>
        [HttpPost("Add")]
        [Authorize]
        public async Task<IActionResult> AddUser(UserEntity new_user)
        {
            var user_ = _userDbContext.Users.FirstOrDefault(user => user.UserName == new_user.UserName);
            if (user_ != null)
            {
                return Ok(new { Success = false, Message = $"使用者名稱:{new_user.UserName} 已經存在於用戶清單中" });
            }

            _userDbContext.Users.Add(new_user);
            _userDbContext.SaveChanges();
            return Ok(new { Success = true });
        }

        /// <summary>
        /// 用戶路由變更
        /// </summary>
        /// <param name="userID">用戶ID</param>
        /// <param name="current_route">當前路由</param>
        /// <returns>路由變更結果</returns>
        [HttpGet("UserRouteChange")]
        public async Task<IActionResult> UserRouteChange(string userID, string current_route)
        {
            //WebsocketMiddleware.UserChangeRoute(userID, current_route);
            //if (current_route == "/map")
            //{
            //    var userEdidingMap= WebsocketMiddleware.EditMapUsers.Where(id => id != userID).ToList();

            //    return Ok(new { isOtherUserEditingMap = userEdidingMap .Count!=0,
            //                    userEditing= userEdidingMap
            //    });
            //}
            //else
            return Ok();
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
