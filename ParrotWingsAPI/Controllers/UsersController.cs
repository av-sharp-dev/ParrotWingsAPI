using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApiContext context, IConfiguration configuration)
        {
            _context= context;
            _configuration= configuration;
        }

        //User registration
        [HttpPost]
        public JsonResult CreateUser(PWUsersRegister userInput)
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
                PasswordSalt = passwordSalt
            };

            _context.UserAccs.Add(newUser);
            _context.SaveChanges();

            return new JsonResult(Ok("Success: " + newUser.Name + " successfully registered"));
        }

        [HttpPost]
        public async Task<ActionResult<string>> Login(PWUsersRegister userInput)
        {   
            //input checks
                var userInDb = _context.UserAccs.Find(userInput.Email);
                if (userInDb == null)
                    return BadRequest("Error: user not found");

                if (!VerifyPasswordHash(userInput.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
                    return BadRequest("Error: wrong password");

            string token = CreateToken(userInDb);
            return Ok(token);
        }


        //Get All Users
        [HttpGet, Authorize]
        public JsonResult GetAllUsers()
        {
            var usersInDb = _context.UserAccs.ToList();
            return new JsonResult(Ok(usersInDb));
        }


        private string CreateToken(PWUsers user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name)
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
            //using (var hmac = new HMACSHA512(passwordSalt))
            using var hmac = new HMACSHA512(passwordSalt);
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }
        }
    }
}
