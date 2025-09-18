using Assignment_For_Full_Stack.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUnitOfWork _unitOfWork;

        public UserController(
            ITokenService tokenService,
            IUnitOfWork unitOfWork)
        {
            _tokenService = tokenService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if user already exists
            var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return BadRequest("User with this email already exists");

            // Check if username already exists
            var existingUsername = await _unitOfWork.UserRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUsername != null)
                return BadRequest("Username already exists");

            // Hash password
            var passwordHash = HashPassword(registerDto.Password);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                EmailConfirmed = true, // Set as needed
                SecurityStamp = Guid.NewGuid().ToString()
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _unitOfWork.UserRepository.GetByEmailAsync(loginDto.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            // Verify password
            if (!VerifyPassword(loginDto.Password, user.PasswordHash))
                return Unauthorized("Invalid email or password");

            // Update last login time
            user.LastLoginTime = DateTime.UtcNow;
            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);

            return Ok(new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var (accessToken, newRefreshToken) = await _tokenService.RefreshAsync(refreshTokenDto.RefreshToken);

                return Ok(new
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpGet("users/{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound("User not found");

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                LastLoginTime = user.LastLoginTime
            });
        }

        
        // Helper methods for password hashing and verification
        private string HashPassword(string password)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[16];
                rng.GetBytes(salt);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);

                byte[] hashBytes = new byte[48];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 32);

                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                if (hashBytes.Length != 48)
                    return false;

                byte[] salt = new byte[16];
                Array.Copy(hashBytes, 0, salt, 0, 16);

                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                byte[] hash = pbkdf2.GetBytes(32);

                for (int i = 0; i < 32; i++)
                {
                    if (hashBytes[i + 16] != hash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenDto.RefreshToken);
            }

            return Ok(new { message = "Logged out successfully" });
        }
    }
    
}