using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProductoRepository
    {
        Task<List<Producto>> ObtenerTodosAsync();
        Task<Producto?> ObtenerPorIdAsync(int id);
        Task<List<Producto>> ObtenerProductosPorIdsAsync(List<int> ids);
        Task CrearProductoAsync(Producto producto);
        Task ActualizarProductoAsync(Producto producto);
        Task EliminarProductoAsync(int id);

        Task ActualizarRangoAsync(List<Producto> productos);

    }
}
