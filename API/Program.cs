using Application.Clientes.Commands;
using Application.Clientes.Validators;
using Application.Commons.Mappings;
using Application.Interfaces;
using Application.Producto.Validators;
using Domain.Repository;
using FluentValidation;
using Infrastructure;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Persistence;

var builder = WebApplication.CreateBuilder(args);

// Configuración Global de Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MappingConfig).Assembly);

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Conexion a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Agregando FluentValidation
builder.Services.AddScoped<IValidator<CrearClienteCommand>, CrearClienteCommandValidator>();
builder.Services.AddScoped<IValidator<ActualizarClienteCommand>, ActualizarClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CrearClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ActualizarClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CrearProductoCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ActualizarProductoCommandValidator>();

// Agregando servicios de Infraestructura
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
