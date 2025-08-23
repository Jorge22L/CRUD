using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IPedidoRepository
    {
        // Define los métodos que el repositorio debe implementar
        Task<List<Pedido>> ObtenerTodosAsync();
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId);
        Task<int> CrearPedidoAsync(Pedido pedido);
        Task<bool> ActualizarPedidoAsync(Pedido pedido);
        Task<bool> EliminarPedidoAsync(int id);
        Task CambiarEstadoPedidoAsync(int id, string nuevoEstado);
        Task CompletarPedidoAsync(int id);
        Task CancelarPedidoAsync(int id);
        Task<bool> ClienteExisteAsync(int clienteId);
    }
}