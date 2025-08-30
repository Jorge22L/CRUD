using Application.Interfaces;
using Application.Pedidos.Commands;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {

        private readonly IPedidoService _pedidoService;

        public PedidosController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var pedidos = await _pedidoService.ObtenerTodosAsync();
            return Ok(pedidos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pedido = await _pedidoService.ObtenerPorIdAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> Post(CrearPedidoCommand command)
        {
            var pedido = await _pedidoService.CrearPedidoAsync(command);
            return CreatedAtAction(nameof(Get), new { id = pedido.PedidoId }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPedido(int id, ActualizarPedidoCommand command)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var pedidoActualizado = await _pedidoService.ActualizarPedidoAsync(id, command);
            if (pedidoActualizado == null)
            {
                return NotFound();
            }

            return Ok(pedidoActualizado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _pedidoService.EliminarPedidoAsync(id);
            if (!eliminado)
            {
                return NotFound( new {mensaje = "Pedido no encontrado"});
            }
            return Ok(new { mensaje = "Pedido eliminado correctamente" });
        }
    }
}
