using Domain.Entities;
using Domain.Repository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class PedidoRepository : IPedidoRepository
    {
        private readonly ApplicationDbContext _context;

        public PedidoRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<Pedido> ActualizarPedidoAsync(int pedidoId, Pedido pedido)
        {
            string? detallesJson = null;

            // Solo crear el JSON si detalles no es nulo
            if(pedido.Detalles != null && pedido.Detalles.Any())
            {
                
                detallesJson = JsonSerializer.Serialize(pedido.Detalles.Select(
                    d => new
                    {
                        ProductoId = d.ProductoId,
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.PrecioUnitario,
                        Descuento = d.Descuento,
                        TieneIVA = d.TieneIVA
                    }
                    ));
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
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.ActualizarPedido @PedidoId, @ClienteId, @FormaPago, @Estado, @DetallesJson", parameters);

                return await ObtenerPorIdAsync(pedidoId);
            }
            catch (Exception e)
            {
                throw new Exception("Error al actualizar el pedido", e);
            }
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
                }
                ));

            var parameters = new[]
            {
                new SqlParameter("@ClienteId", pedido.ClienteId),
                new SqlParameter("@FormaPago", pedido.FormaPago),
                new SqlParameter("@Estado", pedido.Estado ?? "Pendiente"),
                new SqlParameter("@DetallesJson", detallesJson)
            };

            var pedidos = await _context.Pedidos.FromSqlRaw(
                "EXEC dbo.GuardarPedido @ClienteId, @FormaPago, @Estado, @DetallesJson", parameters).ToListAsync();

            var pedidoCreado = pedidos.FirstOrDefault();
            if (pedidoCreado == null)
            {
                throw new Exception("No se pudo crear el pedido.");
            }

            return pedidoCreado.PedidoId;
        }

        public async Task<bool> EliminarPedidoAsync(int id)
        {
            var parameters = new[]
            {
                new SqlParameter("@PedidoId", id),
            };

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.EliminarPedido @PedidoId", parameters);

                return true;
            }
            catch (Exception)
            {
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
