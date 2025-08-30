using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repository
{
    public interface IPedidoRepository
    {
        Task<List<Pedido>> ObtenerTodosAsync();
        Task<Pedido?> ObtenerPorIdAsync(int id);
        Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId);
        Task<int> CrearPedidoAsync(Pedido pedido);
        Task<Pedido> ActualizarPedidoAsync(int pedidoId, Pedido pedido);
        Task<bool> EliminarPedidoAsync(int id);
    }
}
