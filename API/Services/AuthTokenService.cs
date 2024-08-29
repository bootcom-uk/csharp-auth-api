using API.Configuration;
using Extensions.Web;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.Security.Claims;

namespace API.Services
{
    public class AuthTokenService
    {

        internal readonly AuthConfiguration _authConfiguration;

        public AuthTokenService(IConfiguration configuration) {
            _authConfiguration = configuration.Get<AuthConfiguration>()!; 
        }

        public string CreateToken(User user)
        {
            return new SecurityTokenDescriptor()
                .AddClaim(ClaimTypes.Email, user!.EmailAddress)
                .AddClaim(ClaimTypes.Name, user!.Name)
                .AddClaim(ClaimTypes.NameIdentifier, user.Id.ToString())
                .SetExpiry(DateTime.UtcNow.AddMinutes(15))                
                .SetKey(_authConfiguration.Token.Secret)
                .SetAudience(_authConfiguration.Token.Audience)
                .SetIssuer(_authConfiguration.Token.Issuer)
                .CreateToken();
        }

    }
}
