using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using ParrotWingsAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ParrotWingsAPI.Services.TokenGenerators
{
    public class AccessTokenGenerator
    {
        private readonly IConfiguration _configuration;
        //private readonly TokenGenerator _tokenGenerator;

        public AccessTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(PWUsers user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:AccessTokenSecret").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var expiration = Convert.ToDouble(_configuration.GetSection("AppSettings:AccessTokenExpirationMinutes").Value);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiration),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
