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
        public async Task<Pedido?> ActualizarPedidoAsync(int pedidoId, Pedido pedido)
        {
            string? detallesJson = null;

            // Solo crear JSON si hay detalles
            if (pedido.Detalles != null && pedido.Detalles.Any())
            {
                detallesJson = JsonSerializer.Serialize(pedido.Detalles.Select(
                    d => new
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Descuento = d.Descuento,
                        TieneIVA = d.TieneIVA
                    }));
            }

            var parameters = new[]
            {
                new SqlParameter("@PedidoId", pedidoId),
                new SqlParameter("@ClienteId", pedido.ClienteId > 0 ? pedido.ClienteId : DBNull.Value),
                new SqlParameter("@FormaPago", !string.IsNullOrEmpty(pedido.FormaPago) ? pedido.FormaPago : DBNull.Value),
                new SqlParameter("@Estado", !string.IsNullOrEmpty(pedido.Estado) ? pedido.Estado : DBNull.Value),
                new SqlParameter("@DetallesJson", detallesJson ?? (object)DBNull.Value)
            };

            try
            {
                // Debug: Imprimir el JSON para verificar
                Console.WriteLine($"DetallesJson: {detallesJson}");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.ActualizarPedido @PedidoId, @ClienteId, @FormaPago, @Estado, @DetallesJson",
                    parameters);

                return await ObtenerPorIdAsync(pedidoId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar el pedido: {ex.Message}", ex);
            }
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

            if (pedidoCreado == null)
            {
                throw new Exception("Error al crear el pedido.");
            }

            return pedidoCreado.PedidoId;
        }

        public async Task<bool> EliminarPedidoAsync(int id)
        {
            var parameters = new[]
            {
                new SqlParameter("@PedidoId", id)
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.EliminarPedido @PedidoId", parameters);

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error al eliminar el pedido: {ex.Message}");
                return false;
            }
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
