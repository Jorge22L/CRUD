using Application.Interfaces;
using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository;
using MapsterMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System.Data;
using System.Text.Json;

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
            var detallesJson = JsonSerializer.Serialize(pedido.Detalles.Select(
                d => new
                {
                    ProductoId = d.ProductoId,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Descuento = d.Descuento,
                    TieneIVA = d.TieneIVA
                }));

            var parameters = new[]
            {
                new SqlParameter("@ClienteId", pedido.ClienteId),
                new SqlParameter("@FormaPago", pedido.FormaPago),
                new SqlParameter("@Estado", pedido.Estado ?? "Pendiente"),
                new SqlParameter("@DetallesJson", detallesJson)
            };

            var pedidos = await _context.Pedidos.FromSqlRaw
                ("EXEC dbo.GuardarPedido @ClienteId, @FormaPago, @Estado, @DetallesJson", parameters).ToListAsync();

            var pedidoCreado = pedidos.FirstOrDefault();

            if(pedidoCreado == null)
            {
                throw new Exception("Error al crear el pedido.");
            }

            return pedidoCreado.PedidoId;
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
