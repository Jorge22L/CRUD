using Application.Clientes.Commands;
using Application.Clientes.Validators;
using Application.Commons.Mappings;
using Application.Interfaces;
using Application.Producto.Validators;
using Domain.Entities;
using Domain.Repository;
using FluentValidation;
using Infrastructure;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiddlewareCustom;
using Persistence;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuración Global de Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MappingConfig).Assembly);

builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// Conexion a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));

// Configuración de Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configuración de JWT

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtKeyString = builder.Configuration["Jwt:Key"];
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyString));

builder.Services.AddSingleton(jwtSigningKey);

// Autenticación y Autorización
builder.Services.AddAuthentication( opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false, // Para efectos del ejemplo no validamos
        ValidateLifetime = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = jwtSigningKey,

        ClockSkew = TimeSpan.FromMinutes(2),
    };

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            Debug.WriteLine($"[JWT] OnMessageReceived - Authorization header: {auth}");
            if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = auth.Substring("Bearer".Length).Trim().Trim('"');
                context.Token = token;

                Debug.WriteLine($"[JWT] Set context.Token, lenght: {token.Length}");
            }

            return Task.CompletedTask;
        }
    };
});

//builder.Services.AddAuthorization(opt =>
//{
//    //Política global
//    opt.FallbackPolicy = new AuthorizationPolicyBuilder()
//            .RequireAuthenticatedUser()
//            .Build();
//});

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
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// Agregando swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Pedidos",
        Version = "v1",
        Description = "Documentación de la API de Pedidos del Curso de Desarrollo de Aplicaciones Web con ASP.NET Core"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>() 
        }
    });
});

var app = builder.Build();

// Middleware personalizado
app.UseRequestLogging();
app.UseExceptionHandling();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Creando usuario de administración al corre migracion
async Task CrearUsuarioAdmin(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string nombreCompleto = "Tomatito Expasa";
    string userName = "tomatito.expasa";
    string adminEmail = "admin@pedidos.com";
    string adminPassword = "Admin123!";

    // Crear rol Admin si no existe
    if(!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Crear usuario admin si no existe
    var usuarioAdmin = await userManager.FindByEmailAsync(adminEmail);
    if (usuarioAdmin == null)
    {
        usuarioAdmin = new Usuario
        {
            UserName = userName,
            Email = adminEmail,
            NombreCompleto = userName,
        };

        var result = await userManager.CreateAsync(usuarioAdmin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(usuarioAdmin, "Admin");
        }
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CrearUsuarioAdmin(services);
}

app.Run();
