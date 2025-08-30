using Application.Interfaces;
using Application.Pedidos.Commands;
using Application.Pedidos.Queries;
using Domain.Entities;
using Domain.Repository;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
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

            return pedidoActualizado != null ? _mapper.Map<PedidoDto>(pedidoActualizado) : null!;
        }

        public async Task<PedidoDto> CrearPedidoAsync(CrearPedidoCommand command)
        {
            var pedido = _mapper.Map<Pedido>(command);
            var pedidoId = await _pedidoRepository.CrearPedidoAsync(pedido);
            var pedidoCreado = await _pedidoRepository.ObtenerPorIdAsync(pedidoId);
            if (pedidoCreado == null)
            {
                throw new Exception("Error al crear el pedido");
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

        public Task<PedidoDto?> ObtenerPorIdAsync(int id)
        {
            var pedido = _pedidoRepository.ObtenerPorIdAsync(id);
            return pedido != null ? _mapper.Map<Task<PedidoDto?>>(pedido) : null!;
        }

        public async Task<List<PedidoDto>> ObtenerTodosAsync()
        {
            var pedidos = await _pedidoRepository.ObtenerTodosAsync();
            return _mapper.Map<List<PedidoDto>>(pedidos);
        }
    }
}
