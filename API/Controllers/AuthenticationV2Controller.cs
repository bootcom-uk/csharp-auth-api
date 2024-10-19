using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    public class AuthenticationV2Controller : BaseController
    {


        public AuthenticationV2Controller(MongoDatabaseService databaseService, IConfiguration configuration) : base(databaseService, configuration)
        {
            
        }

        [HttpPost("generate-access-code/{audience}")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateAccessCode([FromRoute] string audience, [FromForm] string emailAddress, [FromForm] string deviceId)
        {
            if (!_configuration.TokenConfigurationSection.Audience.Contains(audience))
            {
                return BadRequest("Invalid audience.");
            }

            var user = await _databaseService.GetUserByEmailAddress(emailAddress);

            if (user is null)
            {
                return NotFound();
            }

            await _databaseService.StoreAccessCodeAsync(user, deviceId, audience);

            return NoContent();
        }

        [HttpGet("verify-token-validity")]
        public IActionResult VerifyTokenValidity()
        {
            return NoContent();
        }

        [HttpPost("verify-access-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyAccessCode([FromForm] string deviceId, [FromForm] string accessCode)
        {
            var accessCodeRecord = await _databaseService.GetAccessCode(accessCode, deviceId);
            if (accessCodeRecord == null || accessCodeRecord.AccessCodeExpires.ToUniversalTime() < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired access code.");
            }

            var userId = accessCodeRecord.UserId;

            var jwtToken = GenerateJwtToken(userId, accessCodeRecord.Audience);
            var refreshToken = await _databaseService.StoreRefreshTokenAsync(userId, deviceId, accessCodeRecord.Audience);

            return Ok(new Dictionary<string, string>()
            {
                { "JwtToken", jwtToken },
                { "RefreshToken", refreshToken.Token },
                { "UserId", userId.ToString() }
            });
        }

        private string GenerateJwtToken(ObjectId userId, string audience)
        {
            var privateKey = PemUtils.ImportPrivateKey(_configuration!.TokenConfigurationSection.PrivateKey!);

            var claims = new Dictionary<string, object>()
            {
                { JwtRegisteredClaimNames.Sub, userId.ToString() }
            };


            var creds = new SigningCredentials(new RsaSecurityKey(privateKey), SecurityAlgorithms.RsaSha256);


            var token = new JwtSecurityTokenHandler().CreateToken(new SecurityTokenDescriptor()
            {
                Issuer = _configuration.TokenConfigurationSection!.Issuer,
                Audience = audience,
                Claims = claims,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = creds
            });

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromForm] string refreshToken, [FromForm] string deviceId)
        {
            var refreshTokenDoc = await _databaseService.GetRefreshToken(refreshToken, deviceId);

            if (refreshTokenDoc == null || refreshTokenDoc.Expiry.ToUniversalTime() < DateTime.UtcNow)
            {
                return Unauthorized("Invalid or expired refresh token.");
            }

            var userId = refreshTokenDoc.UserId;

            var newJwtToken = GenerateJwtToken(refreshTokenDoc.UserId, refreshTokenDoc.Audience);
            var newRefreshToken = await _databaseService.StoreRefreshTokenAsync(userId, deviceId, refreshTokenDoc.Audience);

            return Ok(new Dictionary<string, string>()
            {
                { "JwtToken", newJwtToken },
                { "RefreshToken", newRefreshToken.Token },
                { "UserId", userId.ToString() }
            });
        }


    }
}
