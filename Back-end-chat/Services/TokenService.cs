using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Back_end_chat.Services
{
	public class TokenService
	{
		private const string SecretKey = "xjF9@3kdPz!mV7qLsC4w#rT1oE6bY8ZnA2sM0HvL5UjN"; // Replace with env config for security
		private const string Issuer = "TimSoft";
		private const string Audience = "TimSoftApp";

		public string CreateToken(string userId, string username, string role)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, userId),
				new Claim(ClaimTypes.Name, username),
				new Claim(ClaimTypes.Role, role),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var token = new JwtSecurityToken(
				issuer: Issuer,
				audience: Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1),
				signingCredentials: credentials
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
