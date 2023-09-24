using Microsoft.IdentityModel.Tokens;
using Smartway.FileLoaderApi.Contracts;
using Smartway.FileLoaderApi.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Smartway.FileLoaderApi.Services;

public class TokenService : ITokenService
{
    private const int OneTimeTokenLength = 32;
    private readonly SymmetricSecurityKey _key;
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Token:Key"]));
    }

    public string CreateJwtToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email)
        };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds,
            Issuer = _config["Token:Issuer"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    public string CreateTokenForOneTimeLink()
    {
        // Create a secure random number generator
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenBytes = new byte[OneTimeTokenLength];
            rng.GetBytes(tokenBytes);

            // Convert the random bytes to a hexadecimal string
            string token = BitConverter.ToString(tokenBytes).Replace("-", "").ToLower();

            return token;
        }
    }
}
