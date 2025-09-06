using Application.Commons;
using Application.Interfaces;
using Application.Producto.Commands;
using Application.Producto.Queries;
using Domain.Entities;
using Domain.Repository;
using Mapster;
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

        public async Task<ProductoDto?> ActualizarProductoAsync(ActualizarProductoCommand command, int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return null;

            command.Adapt(producto);

            await _productoRepository.ActualizarProductoAsync(producto);
            return _mapper.Map<ProductoDto>(producto);
        }

        public async Task<int> CrearProductoAsync(CrearProductoCommand command)
        {
            var producto = _mapper.Map<Producto>(command);
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

        public async Task<PagedResult<ProductoDto>> ObtenerPaginadosAsync(int page, int pageSize)
        {
            var (items, totalCount) = await _productoRepository.ObtenerPaginadosAsync(page, pageSize);

            var dtoItems = _mapper.Map<List<ProductoDto>>(items);

            return new PagedResult<ProductoDto>
            {
                Items = dtoItems,
                Page = page <= 0 ? 1 : page,
                PageSize = pageSize <= 0 ? 10 : pageSize,
                TotalCount = totalCount
            };

        }

        public async Task<ProductoDto?> ObtenerPorIdAsync(int id)
        {
            var producto = await _productoRepository.ObtenerPorIdAsync(id);
            if (producto == null) return null;

            return _mapper.Map<ProductoDto?>(producto);
        }

        public async Task<List<ProductoDto>> ObtenerTodosAsync()
        {
            var productos = await _productoRepository.ObtenerTodosAsync();

            return _mapper.Map<List<ProductoDto>>(productos);
        }
    }
}
