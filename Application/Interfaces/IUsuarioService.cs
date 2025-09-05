using Application.Usuario.Queries;
using Application.Usuario.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUsuarioService
    {
        Task<RegisterResponse> RegisterAsync(RegisterDto registerDto);
        Task<LoginResponse> LoginAsync(LoginDto loginDto);
    }
}
