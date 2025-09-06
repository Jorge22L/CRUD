using Application.Clientes.Commands;
using Application.Clientes.Validators;
using Application.Commons.Mappings;
using Application.Interfaces;
using Application.Pedidos.Services;
using Application.Producto.Commands;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Repository;
using FluentValidation;
using Infrastructure;
using Infrastructure.Services;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MiddlewareCustom;
using Persistence;
using System.Diagnostics;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
Microsoft.IdentityModel.Logging.IdentityModelEventSource.LogCompleteSecurityArtifact = true;

// Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(MappingConfig).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// DB
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ConexionSQL")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt =>
{
    opt.Password.RequireDigit = true;
    opt.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT - Singleton key
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtKeyString = builder.Configuration["Jwt:Key"];
var jwtSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeyString));
builder.Services.AddSingleton(jwtSigningKey);

// AuthN/AuthZ
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true, // recomendado
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = jwtSigningKey,
        ClockSkew = TimeSpan.FromMinutes(2)
    };

    opt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var auth = context.Request.Headers["Authorization"].ToString();
            Debug.WriteLine($"[JWT] OnMessageReceived - Authorization header: {auth}");

            if (!string.IsNullOrWhiteSpace(auth) && auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = auth.Substring("Bearer ".Length).Trim().Trim('"');
                context.Token = token; // ESTO es clave - fuerza el token limpio
                Debug.WriteLine($"[JWT] Set context.Token, length: {token.Length}");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Debug.WriteLine($"[JWT] OnAuthenticationFailed: {context.Exception.GetType().Name} - {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Debug.WriteLine("[JWT] OnTokenValidated - Token válido.");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Debug.WriteLine($"[JWT] OnChallenge - Error: {context.Error}, Desc: {context.ErrorDescription}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Controllers
builder.Services.AddControllers();

// FluentValidation
builder.Services.AddScoped<IValidator<CrearClienteCommand>, CrearClienteCommandValidator>();
builder.Services.AddScoped<IValidator<ActualizarClienteCommand>, ActualizarClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CrearClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ActualizarClienteCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CrearProductoCommand>();
builder.Services.AddValidatorsFromAssemblyContaining<ActualizarProductoCommand>();

// Servicios/Repos
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();
builder.Services.AddScoped<IReporteService, ReporteService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Pedidos",
        Version = "v1",
        Description = "Documentación de la API"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingrese: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
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
        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() }
    });
});

var app = builder.Build();

// Middlewares
app.UseRequestLogging();
app.UseExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}

app.MapControllers();

async Task CreateAdminUser(IServiceProvider serviceProvider)
{
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@tuapp.com";
    string adminPassword = "Admin123!";

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser { UserName = adminEmail, Email = adminEmail, FullName = "Administrador" };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await CreateAdminUser(services);
}

app.Run();