using Application.Interfaces;
using Application.Producto.Commands;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly IValidator<CrearProductoCommand> _crearProductoCommandValidator;
        private readonly IValidator<ActualizarProductoCommand> _actualizarProductoCommandValidator;

        public ProductosController(IProductoService productoService, IValidator<CrearProductoCommand> crearProductoCommandValidator,
            IValidator<ActualizarProductoCommand> actualizarProductoCommandValidator)
        {
            _productoService = productoService;
            _crearProductoCommandValidator = crearProductoCommandValidator;
            _actualizarProductoCommandValidator = actualizarProductoCommandValidator;
        }

        [HttpGet]
        public async Task<ActionResult<List<Producto>>> Get()
        {
            var productos = await _productoService.ObtenerTodosAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> Get(int id)
        {
            var producto = await _productoService.ObtenerPorIdAsync(id);
            if (producto == null) return NotFound();
            return Ok(producto);
        }

        [HttpPost]
        public async Task<ActionResult<Producto>> Post(CrearProductoCommand command)
        {
            var validation = _crearProductoCommandValidator.Validate(command);
            if (!validation.IsValid) return BadRequest(FormatValidationErrors(validation));
            var id = await _productoService.CrearProductoAsync(command);
            return CreatedAtAction(nameof(Get), new { id }, command);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Producto>> Put(int id, ActualizarProductoCommand command)
        {
            var validation = _actualizarProductoCommandValidator.Validate(command);
            if (!validation.IsValid) return BadRequest(FormatValidationErrors(validation));
            var actualizado = await _productoService.ActualizarProductoAsync(id, command);
            if (!actualizado) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var eliminado = await _productoService.EliminarProductoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        private object FormatValidationErrors(FluentValidation.Results.ValidationResult validationResult)
        {
            var detalles = validationResult.Errors
                .Select(e => new
                {
                    Campo = e.PropertyName,
                    Mensaje = e.ErrorMessage,
                    Codigo = e.ErrorCode,
                    Severidad = e.Severity.ToString()
                }).ToList();

            return detalles;
        }
    }
}
