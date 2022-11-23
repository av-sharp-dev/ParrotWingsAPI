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
using ParrotWingsAPI.Services.TokenValidators;
using Microsoft.EntityFrameworkCore;

namespace ParrotWingsAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApiContext _context;
        private readonly AccessTokenGenerator _accessTokenGenerator;
        private readonly RefreshTokenGenerator _refreshTokenGenerator;
        private readonly RefreshTokenValidator _refreshTokenValidator;
        private readonly PasswordServices _passwordServices;
        private readonly decimal PWRegisterRewardAmnt = 500;
        
        public UsersController(ApiContext context,
                               PasswordServices passwordServices,
                               AccessTokenGenerator accessTokenGenerator,
                               RefreshTokenGenerator refreshTokenGenerator,
                               RefreshTokenValidator refreshTokenValidator)
        {
            _context= context;
            _passwordServices = passwordServices;
            _accessTokenGenerator = accessTokenGenerator;
            _refreshTokenGenerator= refreshTokenGenerator;
            _refreshTokenValidator = refreshTokenValidator;
        }

        [HttpPost, AllowAnonymous]
        public async Task<JsonResult> Registration(UsersRegisteration userInput)
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
        public async Task<JsonResult> Login(UsersLogin userInput)
        {
            var userInDb = await _context.UserAccs.FindAsync(userInput.Email.ToLower());
            
            if (userInDb == null)
                return new JsonResult(BadRequest("Error: user not found"));

            if (!_passwordServices.VerifyPasswordHash(userInput.Password, userInDb.PasswordHash, userInDb.PasswordSalt))
                return new JsonResult(BadRequest("Error: wrong password"));

            var RefreshToken = await _context.UserTokens.FindAsync(userInput.Email.ToLower());

            if (RefreshToken != null)
                return new JsonResult(BadRequest("Error: logout first"));

            AuthenticatedUserResponse response = await Authenticate(userInDb);

            return new JsonResult(Ok(response));
        }

        [HttpPost, AllowAnonymous]
        public async Task<JsonResult> Refresh([FromBody] RefreshRequest refreshRequest)
        {
            if (!ModelState.IsValid)
                return new JsonResult(BadRequest("Error: error during refresh token pending"));

            bool isValidRefreshToken = _refreshTokenValidator.Validate(refreshRequest.RefreshToken);

            if (!isValidRefreshToken)
                return new JsonResult(BadRequest("Error: invalid refresh token"));

            var refreshToken = await _context.UserTokens.FirstOrDefaultAsync(r => r.Token == refreshRequest.RefreshToken);

            if (refreshToken == null)
                return new JsonResult(NotFound("Error: Invalid refresh token"));

            var oldTokenRecords = _context.UserTokens.Where(o => o.Email == refreshToken.Email);
            if (oldTokenRecords.Count() > 0)
            {
                foreach (PWRefreshTokens record in oldTokenRecords)
                {
                    _context.UserTokens.Remove(record);
                }
                await _context.SaveChangesAsync();
            }

            var userInDb = await _context.UserAccs.FirstOrDefaultAsync(u => u.Email == refreshToken.Email);

            if (userInDb == null)
                return new JsonResult(NotFound("Error: User not found"));

            AuthenticatedUserResponse response = await Authenticate(userInDb);

            return new JsonResult(Ok(response));
        }

        [HttpDelete]
        public async Task<JsonResult> Logout()
        {
            string userEmail = HttpContext.User.FindFirstValue(ClaimTypes.Email);
            var oldTokenRecords = _context.UserTokens.Where(o => o.Email == userEmail);
            if (oldTokenRecords == null)
            {
                return new JsonResult(Unauthorized("Error: login first"));
            }

            if (oldTokenRecords.Count() > 0)
            {
                foreach (PWRefreshTokens record in oldTokenRecords)
                {
                    _context.UserTokens.Remove(record);
                }
                await _context.SaveChangesAsync();
            }

            return new JsonResult(Ok("Success: logged out"));
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrentUserName()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb == null)
                return new JsonResult(NotFound("Error: User data not found"));


            return new JsonResult(Ok(userInDb.Name));
        }

        [HttpGet]
        public async Task<JsonResult> GetCurrentUserBalance()
        {
            var userInDb = await getCurrentUserFromDB();

            if (userInDb == null)
                return new JsonResult(NotFound("Error: User data not found"));

            return new JsonResult(Ok(userInDb.Balance));
        }

        private async Task<PWUsers> getCurrentUserFromDB()
        {
            var userIdentity = User.FindFirstValue(ClaimTypes.Email);
            var userInDb = await _context.UserAccs.FindAsync(userIdentity);
            return userInDb;
        }

        private async Task<AuthenticatedUserResponse> Authenticate(PWUsers user)
        {
            string newAccessToken = _accessTokenGenerator.GenerateToken(user);
            string newRefreshToken = _refreshTokenGenerator.GenerateToken();

            PWRefreshTokens refreshToken = new PWRefreshTokens()
            {
                Token = newRefreshToken,
                Email = user.Email
            };
            await _context.UserTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticatedUserResponse()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}
