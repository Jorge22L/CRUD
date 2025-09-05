using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public interface IUsuarioRepository
    {
        Task<IdentityResult> RegistrarUsuarioAsync(Usuario usuario, string password);
        Task<SignInResult> LoginUsuarioAsync(string email, string password);
        Task<Usuario> ObtenerUsuarioPorEmailAsync(string email);
    }
}
