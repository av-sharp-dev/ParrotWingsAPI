using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using ParrotWingsAPI.Data;
using ParrotWingsAPI.Models;
using ParrotWingsAPI.Models.Requests;
using ParrotWingsAPI.Models.Responses;
using ParrotWingsAPI.Services.TokenGenerators;
using ParrotWingsAPI.Services.PasswordServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly IConfiguration _configuration;
        private readonly AccessTokenGenerator _accessTokenGenerator;
        private readonly PasswordServices _passwordServices;
        private readonly decimal PWRegisterRewardAmnt = 500;

        public UsersController(ApiContext context, IConfiguration configuration, AccessTokenGenerator accessTokenGenerator, PasswordServices passwordServices)
        {
            _context= context;
            _configuration= configuration;
            _accessTokenGenerator= accessTokenGenerator;
            _passwordServices = passwordServices;
        }

        [HttpPost, AllowAnonymous]
        public async Task<JsonResult> Registration(PWUsersRegisteration userInput)
        {
            var userInDb = await _context.UserAccs.FindAsync(userInput.Email.ToLower());

            if (userInDb != null)
                return new JsonResult(NotFound("Error: user with this email already registered"));

            _passwordServices.CreatePasswordHash(userInput.Password, out byte[] passwordHash, out byte[] passwordSalt);

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

            if (!_passwordServices.VerifyPasswordHash(userInput.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
                return new JsonResult(BadRequest("Error: wrong password"));


            string token = _accessTokenGenerator.GenerateToken(userInDb);
            userInDb.IsLoggedIn = true;
            await _context.SaveChangesAsync();

            return new JsonResult(Ok(new AuthenticatedUserResponse()
            {
                AccessToken= token
            }));
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
    }
}
