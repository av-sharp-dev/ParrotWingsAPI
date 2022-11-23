using System.Security.Claims;

namespace ParrotWingsAPI.Services.TokenGenerators
{
    public class RefreshTokenGenerator
    {
        private readonly IConfiguration _configuration;
        private readonly TokenGenerator _tokenGenerator;

        public RefreshTokenGenerator(IConfiguration configuration, TokenGenerator tokenGenerator)
        {
            _configuration = configuration;
            _tokenGenerator = tokenGenerator;
        }
        public string GenerateToken()
        {



            return _tokenGenerator.GenerateToken(
                _configuration.GetSection("AppSettings:RefreshTokenSecret").Value,
                Convert.ToInt16(_configuration.GetSection("AppSettings:RefreshTokenExpirationMinutes").Value)
                );
        }
    }
}
