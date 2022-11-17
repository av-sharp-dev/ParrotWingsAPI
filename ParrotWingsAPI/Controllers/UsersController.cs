using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApiContext context, IConfiguration configuration)
        {
            _context= context;
            _configuration= configuration;
        }

        [HttpPost]
        public JsonResult Registration(PWUsersRegisteration userInput)
        {
            var userInDb = _context.UserAccs.Find(userInput.Email);
                
            if (userInDb != null)
                return new JsonResult(NotFound("Error: user with this email already registered"));

            CreatePasswordHash(userInput.Password, out byte[] passwordHash, out byte[] passwordSalt);
            
            var newUser = new PWUsers
            {
                Email = userInput.Email,
                Name = userInput.Name,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Balance = 500
            };

            _context.UserAccs.Add(newUser);
            _context.SaveChanges();

            return new JsonResult(Ok("Success: " + newUser.Name + " successfully registered and awarded with 500 starting PW balance"));
        }

        [HttpPost]
        public JsonResult Login(PWUsersLogin userInput)
        {   
            var userInDb = _context.UserAccs.Find(userInput.Email);

            if (userInDb == null)
                return new JsonResult(BadRequest("Error: user not found"));

            if (!VerifyPasswordHash(userInput.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
                return new JsonResult(BadRequest("Error: wrong password"));

            string token = CreateToken(userInDb);
            return new JsonResult(Ok(token));
        }

        [HttpGet, Authorize]
        public JsonResult GetUserName()
        {
            var userInDb = _context.UserAccs.Find(CurrentUserEmail());
            
            if (userInDb == null)
                return new JsonResult(NotFound("Error: internal server error. User data not found"));
            
            return new JsonResult (Ok(userInDb.Name));
        }

        [HttpGet, Authorize]
        public JsonResult GetUserBalance()
        {
            var userInDb = _context.UserAccs.Find(CurrentUserEmail());

            if (userInDb == null)
                return new JsonResult(NotFound("Error: internal server error. User data not found"));

            return new JsonResult(Ok(userInDb.Balance));
        }

        [HttpGet, Authorize]
        public JsonResult GetAllUsers()
        {
            var usersInDb = _context.UserAccs.ToList();
            return new JsonResult(Ok(usersInDb));
        }

        private string CurrentUserEmail()
        {
            return User.FindFirstValue(ClaimTypes.Email);
        }

        private string CreateToken(PWUsers user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
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
