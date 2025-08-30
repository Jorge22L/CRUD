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
        public async Task<PedidoDto> ActualizarPedidoAsync(int id, ActualizarPedidoCommand command)
        {
            var pedido = _mapper.Map<Pedido>(command);
            var pedidoActualizado = await _pedidoRepository.ActualizarPedidoAsync(id, pedido);

            return pedidoActualizado != null ? _mapper.Map<PedidoDto>(pedidoActualizado) : null;
        }

        public async Task<bool> CambiarEstadoPedidoAsync(int id, string nuevoEstado)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CancelarPedidoAsync(int id)
        {
            throw new NotImplementedException();
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
            return await _pedidoRepository.EliminarPedidoAsync(id);
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
