using Application.Interfaces;
using Application.Producto.Commands;
using Application.Producto.Queries;
using Domain.Interfaces;
using MapsterMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly IMapper _mapper;

        public ProductoService(IProductoRepository productoRepository, IMapper mapper)
        {
            _productoRepository = productoRepository;
            _mapper = mapper;
        }

        public async Task<bool> ActualizarProductoAsync(int id, ActualizarProductoCommand command)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return false;

            _mapper.Map(command, producto);

            await _productoRepository.ActualizarProductoAsync(producto);
            return true;
        }

        public async Task<int> CrearProductoAsync(CrearProductoCommand command)
        {
            var producto = _mapper.Map<Domain.Entities.Producto>(command);

            await _productoRepository.CrearProductoAsync(producto);
            return producto.ProductoId;
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return false;

            await _productoRepository.EliminarProductoAsync(id);
            return true;
        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return null;
            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<List<ProductoDto>> ObtenerTodosAsync()
        {
            var productos = await _productoRepository.ObtenerTodosAsync();
            return _mapper.Map<List<ProductoDto>>(productos);
        }
    }
}
