using Application.Clientes.Commands;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClientesController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        // https://localhost:7106/api/Clientes
        public async Task<IActionResult> Get()
        {
            var clientes = await _clienteService.ObtenerTodosAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        // https://localhost:7106/api/Clientes/{id}
        public async Task<ActionResult<List<Cliente>>> Get(int id)
        {
            var cliente = await _clienteService.ObtenerPorIdAsync(id);
            return Ok(cliente);
        }

        [HttpPost]
        public async Task<ActionResult<Cliente>> Post(CrearClienteCommand command)
        {
            var id = await _clienteService.CrearClienteAsync(command);
            return CreatedAtAction(nameof(Get), new { id }, command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Cliente>> Put(int id, ActualizarClienteCommand command)
        {
            var actualizado = await _clienteService.ActualizarClienteAsync(id, command);
            if (!actualizado) return NotFound();

            return Ok(actualizado);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Cliente>> Delete(int id)
        {
            var eliminado = await _clienteService.EliminarClienteAsync(id);
            if (!eliminado) return NotFound();

            return Ok(eliminado);
        }
        
    }
}
