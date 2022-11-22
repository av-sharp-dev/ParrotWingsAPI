using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;
        private readonly decimal PWRegisterRewardAmnt = 500;

        public UsersController(ApiContext context, IConfiguration configuration)
        {
            _context= context;
            _configuration= configuration;
        }

        [HttpPost, AllowAnonymous]
        public async Task<JsonResult> Registration(PWUsersRegisteration userInput)
        {
            var userInDb = await _context.UserAccs.FindAsync(userInput.Email.ToLower());
                
            if (userInDb != null)
                return new JsonResult(NotFound("Error: user with this email already registered"));

            CreatePasswordHash(userInput.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            var newUser = new PWUsers
            {
                Email = userInput.Email.ToLower(),
                Name = userInput.Name,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Balance = PWRegisterRewardAmnt
            };

            await _context.UserAccs.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return new JsonResult(Ok("Success: " + newUser.Name + " successfully registered and awarded with 500 starting PW balance"));
        }

        [HttpPost, AllowAnonymous]
        public async Task<JsonResult> Login(PWUsersLogin userInput)
        {
            var userInDb = await _context.UserAccs.FindAsync(userInput.Email.ToLower());

            if (userInDb == null)
                return new JsonResult(BadRequest("Error: user not found"));

            if (!VerifyPasswordHash(userInput.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
                return new JsonResult(BadRequest("Error: wrong password"));

            string token = CreateToken(userInDb);

            userInDb.IsLoggedIn = true;
            await _context.SaveChangesAsync();

            return new JsonResult(Ok(token));
        }

        [HttpPost]
        public async Task<JsonResult> Logout()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(BadRequest("Error: already logged out"));

            userInDb.IsLoggedIn = false;
            await _context.SaveChangesAsync();

            return new JsonResult(Ok("Success: logged out"));
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrentUserName()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb == null)
                return new JsonResult(NotFound("Error: internal server error. User data not found"));

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            return new JsonResult(Ok(userInDb.Name));
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrentUserBalance()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb == null)
                return new JsonResult(NotFound("Error: internal server error. User data not found"));

            if (userInDb.IsLoggedIn == false)
                return new JsonResult(Unauthorized("Error: login required"));

            return new JsonResult(Ok(userInDb.Balance));
        }

        private async Task<PWUsers> getCurrentUserFromDB()
        {
            var userIdentity = User.FindFirstValue(ClaimTypes.Email);
            var userInDb = await _context.UserAccs.FindAsync(userIdentity);
            return userInDb;
        }

        private string CreateToken(PWUsers user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);
            var jsonWebToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jsonWebToken;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
