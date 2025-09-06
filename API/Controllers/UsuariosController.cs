using Application.Interfaces;
using Application.Usuario.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Registrar(RegisterDto registerDto)
        {
            var result = await _usuarioService.RegisterAsync(registerDto);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var result = await _usuarioService.LoginAsync(loginDto);
            if(!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(new { token = result.Token});
        }
    }
}
