using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Notebook.Core.AppSettings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Notebook.Core.Services
{
    public class TokenService
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly AuthTokenParameters authTokenParameters;

        public TokenService(UserManager<IdentityUser> userManager, IOptions<AuthTokenParameters> authTokenParameters)
        {
            this.userManager = userManager;
            this.authTokenParameters = authTokenParameters.Value;
        }

        public async Task<string> CreateToken(IdentityUser user)
        {
            var roles = await this.userManager.GetRolesAsync(user);
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authTokenParameters.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(60),
                SigningCredentials = credentials,
                Issuer = this.authTokenParameters.Issuer,
                Audience = this.authTokenParameters.Audience,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}