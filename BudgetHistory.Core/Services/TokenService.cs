using BudgetHistory.Core.AppSettings;
using BudgetHistory.Core.Constants;
using BudgetHistory.Core.Models;
using BudgetHistory.Core.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BudgetHistory.Core.Services
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AuthTokenParameters _authTokenParameters;

        public TokenService(UserManager<IdentityUser> userManager, IOptions<AuthTokenParameters> authTokenParameters)
        {
            _userManager = userManager;
            _authTokenParameters = authTokenParameters.Value;
        }

        public async Task<string> CreateAuthTokenAsync(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            PrepareTokenData(claims, out JwtSecurityTokenHandler tokenHandler, out SecurityToken token, _authTokenParameters.TokenExpirationTimeInHours * 60);

            return tokenHandler.WriteToken(token);
        }

        public string CreateRoomSessionToken(Room room, string userId)
        {
            var claims = new List<Claim>()
            {
                new Claim(ClaimConstants.RoomName, room.Name),
                new Claim(ClaimConstants.RoomId, room.Id.ToString()),
                new Claim(ClaimConstants.UserId, userId),
            };

            PrepareTokenData(claims, out JwtSecurityTokenHandler tokenHandler, out SecurityToken token, Cookies.RoomTokenExpirationInMinutes);

            return tokenHandler.WriteToken(token);
        }

        public IEnumerable<Claim> DecodeToken(string authToken)
        {
            var jwtToken = authToken.Split(" ").Last();
            var handler = new JwtSecurityTokenHandler();
            return handler.ReadJwtToken(jwtToken).Claims;
        }

        private void PrepareTokenData(List<Claim> claims, out JwtSecurityTokenHandler tokenHandler, out SecurityToken token, int expirationTimeInMinutes)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authTokenParameters.SigningKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMinutes(expirationTimeInMinutes),
                SigningCredentials = credentials,
                Issuer = _authTokenParameters.Issuer,
                Audience = _authTokenParameters.Audience,
            };

            tokenHandler = new JwtSecurityTokenHandler();
            token = tokenHandler.CreateToken(tokenDescriptor);
        }
    }
}