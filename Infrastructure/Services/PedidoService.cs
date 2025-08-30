using Application.DetallePedido.Queries;
using Application.Interfaces;
using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Pedidos.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public PedidoService(IPedidoRepository pedidoRepository, IProductoRepository productoRepository, IMapper mapper)
        {
            _pedidoRepository = pedidoRepository;
            _productoRepository = productoRepository;
            _mapper = mapper;
        }
        public async Task<bool> ActualizarPedidoAsync(int id, ActualizarPedidoCommand command)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            if (pedido == null) return false;

            if(pedido.Estado != "Pendiente")
            {
                throw new Exception("Solo se pueden actualizar pedidos en estado 'Pendiente'.");
            }

            // Restaurar stock de detalles existentes
            var productosOriginales = await _productoRepository.ObtenerProductosPorIdsAsync(
                pedido.Detalles.Select(d => d.ProductoId).ToList());

            foreach(var detalle in pedido.Detalles)
            {
                var producto = productosOriginales.First(p => p.ProductoId == detalle.ProductoId);
                producto.Existencias += detalle.Cantidad;
            }

            // Mapear nuevos detalles
            _mapper.Map(command, pedido);

            if(command.Detalles != null && command.Detalles.Any())
            {
                pedido.Detalles.Clear();

                var productosNuevos = await _productoRepository.ObtenerProductosPorIdsAsync(
                    command.Detalles.Select(d => d.ProductoId).ToList());

                foreach(var detalle in command.Detalles)
                {
                    var producto = productosNuevos.First(p => p.ProductoId == detalle.ProductoId);
                    if(producto.Existencias < detalle.Cantidad)
                    {
                        throw new Exception($"No hay stock suficiente para el producto {producto.Nombre}.");
                    }
                    var detallePedido = _mapper.Map<Domain.Entities.DetallePedido>(detalle);
                    pedido.Detalles.Add(detallePedido);
                    producto.Existencias -= detalle.Cantidad;
                }

                await _productoRepository.ActualizarRangoAsync(productosOriginales.Concat(productosNuevos).ToList());
            }

            CalcularTotalesPedido(pedido);
            await _pedidoRepository.ActualizarPedidoAsync(pedido);

            return true;
        }

        public async Task<bool> CambiarEstadoPedidoAsync(int id, string nuevoEstado)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            if(pedido == null ) return false;

            var estadosValidos = new List<string> { "Pendiente", "Procesando", "Completado", "Cancelado" };
            if(!estadosValidos.Contains(nuevoEstado))
            {
                throw new Exception("Estado no válido.");
            }

            pedido.Estado = nuevoEstado;
            await _pedidoRepository.ActualizarPedidoAsync(pedido);
            return true;
        }

        public async Task<bool> CancelarPedidoAsync(int id)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            if(pedido == null) return false;
            if(pedido.Estado == "Pendiente")
            {
                var productos = await _productoRepository.ObtenerProductosPorIdsAsync(
                    pedido.Detalles.Select(d => d.ProductoId).ToList());

                foreach(var detalle in pedido.Detalles)
                {
                    var producto = productos.First(p => p.ProductoId == detalle.ProductoId);
                    producto.Existencias += detalle.Cantidad;
                }

                await _productoRepository.ActualizarRangoAsync(productos);
            }

            pedido.Estado = "Cancelado";
            await _pedidoRepository.ActualizarPedidoAsync(pedido);
            return true;
        }

        public async Task<bool> CompletarPedidoAsync(int id)
        {
            return await CambiarEstadoPedidoAsync(id, "Completado");
        }

        public async Task<PedidoDto> CrearPedidoAsync(CrearPedidoCommand command)
        {
            var pedido = _mapper.Map<Pedido>(command);

            var pedidoId = await _pedidoRepository.CrearPedidoAsync(pedido);

            var pedidoCreado = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);

            if (pedidoCreado == null)
            {
                throw new Exception("Error al crear el pedido.");
            }

            return _mapper.Map<PedidoDto>(pedidoCreado);
        }

        public async Task<bool> EliminarPedidoAsync(int id)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            if(pedido == null) return false;

            if(pedido.Estado != "Pendiente")
            {
                throw new InvalidOperationException("Solo se pueden eliminar pedidos en estado 'Pendiente'.");
            }

            var productos = await _productoRepository.ObtenerProductosPorIdsAsync(
                pedido.Detalles.Select(d => d.ProductoId).ToList());

            foreach(var detalle in pedido.Detalles)
            {
                var producto = productos.First(p => p.ProductoId == detalle.ProductoId);
                producto.Existencias += detalle.Cantidad;
            }

            await _productoRepository.ActualizarRangoAsync(productos);
            await _pedidoRepository.EliminarPedidoAsync(id);

            return true;
        }

        public async Task<List<PedidoDto>> ObtenerPorClienteAsync(int clienteId)
        {
            var pedidos = await _pedidoRepository.ObtenerPorClienteAsync(clienteId);
            return _mapper.Map<List<PedidoDto>>(pedidos);
        }

        public async Task<PedidoDto?> ObtenerPorIdAsync(int id)
        {
            var pedido = await _pedidoRepository.ObtenerPorIdAsync(id);
            return pedido == null ? null : _mapper.Map<PedidoDto>(pedido);
        }

        public async Task<List<PedidoDto>> ObtenerTodosAsync()
        {
            var pedidos = await _pedidoRepository.ObtenerTodosAsync();
            return _mapper.Map<List<PedidoDto>>(pedidos);
        }

        private void CalcularTotalesPedido(Pedido pedido)
        {
            decimal subtotal = 0;
            decimal totalIVA = 0;

            foreach (var detalle in pedido.Detalles)
            {
                var subtotalLinea = (detalle.Cantidad * detalle.PrecioUnitario) - detalle.Descuento;
                subtotal += subtotalLinea;

                if (detalle.TieneIVA)
                    totalIVA += subtotalLinea * 0.15m;
            }

            pedido.SubTotal = subtotal;
            pedido.IVA = totalIVA;
            pedido.Total = subtotal + totalIVA - pedido.Descuento;
        }

      
    }
}
