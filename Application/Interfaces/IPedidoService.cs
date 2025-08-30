using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPedidoService
    {
        Task<PedidoDto> CrearPedidoAsync(CrearPedidoCommand command);
        Task<PedidoDto> ActualizarPedidoAsync(int id, ActualizarPedidoCommand command);
        Task<bool> EliminarPedidoAsync(int id);
        Task<bool> CambiarEstadoPedidoAsync(int id, string nuevoEstado);
        Task<bool> CancelarPedidoAsync(int id);
        Task<bool> CompletarPedidoAsync(int id);
        Task<List<PedidoDto>> ObtenerTodosAsync();
        Task<List<PedidoDto>> ObtenerPorClienteAsync(int clienteId);
        Task<PedidoDto?> ObtenerPorIdAsync(int id);
    }
}
