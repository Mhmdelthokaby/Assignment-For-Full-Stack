using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Repository.Data;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class TokenService : ITokenService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public TokenService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var accessToken = CreateAccessToken(user);
            var refreshToken = CreateRefreshToken();

            var rt = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(jwt["RefreshTokenDays"] ?? "30")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ReplacedByToken = null // Explicitly set to null
            };

            _db.Set<RefreshToken>().Add(rt);
            await _db.SaveChangesAsync();

            return (accessToken, refreshToken);
        }
        public async Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshToken)
        {
            var existing = await _db.Set<RefreshToken>().FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (existing == null || existing.IsRevoked || existing.ExpiresAt < DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            var user = await _db.Users.FindAsync(existing.UserId);
            if (user == null) throw new SecurityTokenException("User not found");

            // revoke old and create new
            existing.IsRevoked = true;
            existing.ReplacedByToken = CreateRefreshToken();
            await _db.SaveChangesAsync();

            var accessToken = CreateAccessToken(user);

            var rtNew = new RefreshToken
            {
                UserId = user.Id,
                Token = existing.ReplacedByToken,
                ExpiresAt = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenDays"] ?? "30")),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                ReplacedByToken = null // Explicitly set to null for new tokens
            };
            _db.Set<RefreshToken>().Add(rtNew);
            await _db.SaveChangesAsync();

            return (accessToken, rtNew.Token);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var existing = await _db.Set<RefreshToken>().FirstOrDefaultAsync(r => r.Token == refreshToken);
            if (existing == null) return;
            existing.IsRevoked = true;
            await _db.SaveChangesAsync();
        }

        private string CreateAccessToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };

            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["AccessTokenMinutes"] ?? "15")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    
}

