using Application.Interfaces;
using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using Domain.Entities;
using Domain.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Services
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> ActualizarPedidoAsync(Pedido pedido)
        {
            _context.Entry(pedido).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task CambiarEstadoPedidoAsync(int id, string nuevoEstado)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido != null)
            {
                pedido.Estado = nuevoEstado;
                await _context.SaveChangesAsync();
            }
        }

        public async Task CancelarPedidoAsync(int id)
        {
            await CambiarEstadoPedidoAsync(id, "Cancelado");
        }

        public async Task<bool> ClienteExisteAsync(int clienteId)
        {
            return await _context.Clientes.AnyAsync(c => c.ClienteID == clienteId);
        }

        public async Task CompletarPedidoAsync(int id)
        {
            await CambiarEstadoPedidoAsync(id, "Completado");
        }

        public async Task<int> CrearPedidoAsync(Pedido pedido)
        {
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync();
            return pedido.PedidoId;
        }

        public async Task<bool> EliminarPedidoAsync(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null) return false;
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Pedido>> ObtenerPorClienteAsync(int clienteId)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(p => p.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<Pedido?> ObtenerPorIdAsync(int id)
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoId == id);
        }

        public async Task<List<Pedido>> ObtenerTodosAsync()
        {
            return await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                .ToListAsync();
        }
    }
}
