using Application.ApplicationUser;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public IdentityController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            var result = await _identityService.RegisterAsync(registerDto);
            if (!result.Success)
                return BadRequest(result.Errors);

            return Ok("User registered successfully");
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _identityService.LoginAsync(loginDto);
            if (!result.Success)
                return Unauthorized(result.Errors);

            return Ok(new { token = result.Token });
        }

        [AllowAnonymous]
        [HttpGet("debug-validate")]
        public IActionResult DebugValidate([FromServices] IConfiguration cfg,
                                   [FromServices] SymmetricSecurityKey signingKey)
        {
            var auth = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrWhiteSpace(auth) || !auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Missing Bearer");

            var token = auth.Substring("Bearer ".Length).Trim().Trim('"');

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(2)
                }, out var validatedToken);

                return Ok(new
                {
                    Message = "Manual validation OK",
                    Name = principal.Identity?.Name,
                    HeaderAlg = ((JwtSecurityToken)validatedToken).Header.Alg,
                    Iss = ((JwtSecurityToken)validatedToken).Issuer,
                    Exp = ((JwtSecurityToken)validatedToken).ValidTo
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Manual validation FAILED", Error = ex.Message });
            }
        }
    }
}