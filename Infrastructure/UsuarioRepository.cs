using Domain.Entities;
using Domain.Repository;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public UsuarioRepository(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<SignInResult> LoginUsuarioAsync(string email, string password)
        {
            var usuario = await _userManager.FindByEmailAsync(email);
            if (usuario == null)
            {
                return SignInResult.Failed;
            }
            return await _signInManager.CheckPasswordSignInAsync(usuario, password,false);
        }

        public async Task<Usuario> ObtenerUsuarioPorEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<IdentityResult> RegistrarUsuarioAsync(Usuario usuario, string password)
        {
            return await _userManager.CreateAsync(usuario, password);
        }
    }
}
