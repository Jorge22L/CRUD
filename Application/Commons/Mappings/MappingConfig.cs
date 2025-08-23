using Application.Clientes.Commands;
using Application.Clientes.Queries;
using Application.Producto.Commands;
using Application.Producto.Queries;
using Domain.Entities;
using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commons.Mappings
{
    public class MappingConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // Entidad <-> DTO
            config.NewConfig<Cliente, ClienteDto>();
            config.NewConfig<ClienteDto, Cliente>();


            // Command <-> Entidad
            config.NewConfig<CrearClienteCommand, Cliente>();
            config.NewConfig<ActualizarClienteCommand, Cliente>();

            config.NewConfig<Domain.Entities.Producto, ProductoDto>();
            config.NewConfig<ProductoDto, Domain.Entities.Producto>();

            config.NewConfig<CrearProductoCommand, Domain.Entities.Producto>();
            config.NewConfig<ActualizarProductoCommand, Domain.Entities.Producto>();
        }
    }
}
