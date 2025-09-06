using Application.ApplicationUser;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;

        public IdentityService(IUserRepository userRepository, IConfiguration configuration, SymmetricSecurityKey key)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _key = key;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FullName = registerDto.FullName
            };

            var result = await _userRepository.RegisterUserAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                };
            }

            return new RegisterResponseDto { Success = true };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var result = await _userRepository.LoginUserAsync(loginDto.Email, loginDto.Password);

            if (!result.Succeeded)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Errors = new[] { "Invalid email or password" }
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);
            var token = GenerateJwtToken(user);

            return new LoginResponseDto { Success = true, Token = token };
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(60),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}