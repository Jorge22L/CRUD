using Application.Producto.Commands;
using Application.Producto.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IProductoService
    {
        Task<List<ProductoDto>> ObtenerTodosAsync();
        Task<ProductoDto?> ObtenerPorIdAsync(int id);
        Task<int> CrearProductoAsync(CrearProductoCommand command);
        Task<ProductoDto> ActualizarProductoAsync(ActualizarProductoCommand command, int id);
        Task<bool> EliminarProductoAsync(int id);
    }
}
