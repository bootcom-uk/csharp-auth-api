using API.Communication;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace API.Controllers
{
    public class AuthenticationV2Controller : BaseController
    {


        public AuthenticationV2Controller(MongoDatabaseService databaseService, IConfiguration configuration, EmailProviderService emailProviderService) : base(databaseService, configuration, emailProviderService)
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

            var accessCodeRequest = await _databaseService.StoreAccessCodeAsync(user, deviceId, audience);


            await _emailProviderService.Send(_configuration.EmailConfigurationSection.EmailFrom, "Login To BootCom", _configuration.EmailConfigurationSection.LoginTemplate, new Dictionary<string, string>()
            {
                { "{{BASE_URL}}", _configuration.EmailConfigurationSection.LoginBaseURL },
                { "{{ACCESS_CODE}}", accessCodeRequest!.QuickAccessCode},
                { "{{LOGIN_CODE}}", accessCodeRequest!.LoginCode.ToString()}
            }, new List<Exchange365EmailRecipient>()
            {
                { new() { Contact = emailAddress, Name = emailAddress} }
            });

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

            return Ok(new
            {
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token
            });
        }

        private string GenerateJwtToken(ObjectId userId, string audience)
        {
            var privateKey = RSA.Create();
            privateKey.ImportPkcs8PrivateKey(Convert.FromBase64String(_configuration!.TokenConfigurationSection.PrivateKey!), out _);

            var claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Aud, audience)
            };


            var creds = new SigningCredentials(new RsaSecurityKey(privateKey), SecurityAlgorithms.RsaSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration.TokenConfigurationSection!.Issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: creds
            );

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

            return Ok(new
            {
                JwtToken = newJwtToken,
                RefreshToken = newRefreshToken
            });
        }


    }
}
