using Application.Interfaces;
using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
        [HttpGet("{id}/reporte-fastreport/pdf")]
        public async Task<IActionResult> ReporteFastReportPdf(int id, [FromServices] IReporteService fastReportService)
        {
            var bytes = await fastReportService.GenerarDetallePedidoPdfAsync(id);
            return File(bytes, "application/pdf", $"pedido_{id}_fr.pdf");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pedido = await _pedidoService.ObtenerPorIdAsync(id);
            if (pedido == null) return NotFound();
            return Ok(pedido);
        }

        [HttpPost]
        public async Task<ActionResult<PedidoDto>> Post(CrearPedidoCommand command)
        {
            var pedido = await _pedidoService.CrearPedidoAsync(command);
            return CreatedAtAction(nameof(Get), new { id = pedido.PedidoId }, pedido);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> ActualizarPedido(int id, [FromBody] ActualizarPedidoCommand command)
        {
            if (!ModelState.IsValid)
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
        public async Task<IActionResult> EliminarPedido(int id)
        {
            var result = await _pedidoService.EliminarPedidoAsync(id);
            if (!result)
            {
                return NotFound(new {mensaje = "Pedido no encontrado"});
            }
            return Ok(new {mensaje = "Pedido eliminado exitosamente"});
        }
    }
}
