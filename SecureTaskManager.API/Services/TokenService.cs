using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureTaskManager.API.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SecureTaskManager.API.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");

            var secretKey = jwtSettings["SecretKey"] ?? throw new ArgumentNullException("JwtSettings:SecretKey está ausente.");
            var issuer = jwtSettings["Issuer"] ?? throw new ArgumentNullException("JwtSettings:Issuer está ausente.");
            var audience = jwtSettings["Audience"] ?? throw new ArgumentNullException("JwtSettings:Audience está ausente.");
            var expirationMinutes = jwtSettings["ExpirationInMinutes"] ?? throw new ArgumentNullException("JwtSettings:ExpirationInMinutes está ausente.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Busca as roles reais do usuário no banco
            var roles = await _userManager.GetRolesAsync(user);

            // Cria a lista de claims básicas
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            };

            // Adiciona uma claim de role para cada role do usuário
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Cria o token JWT com as claims e dados do JWT
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(expirationMinutes)),
                signingCredentials: creds
            );

            // Retorna o token no formato string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }



    }
}
