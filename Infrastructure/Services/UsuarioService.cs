using Application.Interfaces;
using Application.Usuario.Queries;
using Application.Usuario.Response;
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
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguration _configuration;
        private readonly SymmetricSecurityKey _key;

        public UsuarioService(IUsuarioRepository usuarioRepository, IConfiguration configuration, SymmetricSecurityKey key)
        {
            _usuarioRepository = usuarioRepository;
            _configuration = configuration;
            _key = key;
        }
        public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
        {
            var result = await _usuarioRepository.LoginUsuarioAsync(loginDto.Email, loginDto.Password);
            if(!result.Succeeded)
            {
                return new LoginResponse
                {
                    Success = false,
                    Errors = new[] {"Usuario o contraseña incorrectos" }
                };
            }

            var user = await _usuarioRepository.ObtenerUsuarioPorEmailAsync(loginDto.Email);
            var token = GenerateJwtToken(user);

            return new LoginResponse { Success = true,
                Token = token
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterDto registerDto)
        {
            var usuario = new Usuario
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                NombreCompleto = registerDto.NombreCompleto
            };

            var result = await _usuarioRepository.RegistrarUsuarioAsync(usuario, registerDto.Password);
            if(!result.Succeeded)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                };
            }

            return new RegisterResponse { Success = true };

        }

        private string GenerateJwtToken(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto)
            };

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);


            return tokenHandler.WriteToken(token);
        }
    }
}
