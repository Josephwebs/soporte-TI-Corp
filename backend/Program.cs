using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketManager.API.Application.Services;
using TicketManager.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Configure DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("TicketDB"));

// Services
builder.Services.AddScoped<ITicketService, TicketService>();

// ... Auth ...
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "TicketSystem",
            ValidAudience = "TicketSystemClients",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ClaveSuperSecretaDeDesarrolloMinimo32Caracteres!"))
        };
    });

builder.Services.AddAuthorization();

// CORS Endurecido (Evitando AllowAll en producción)
builder.Services.AddCors(options =>
{
    options.AddPolicy("StrictCors", builder =>
        builder.WithOrigins("http://localhost:5173", "https://tudominio.com")
               .AllowAnyMethod()
               .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TicketManager.API.Middleware.ExceptionMiddleware>();

app.UseCors("StrictCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// === Seed Data (Para pruebas prácticas) ===
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Tickets.Any())
    {
        context.Tickets.AddRange(
            new TicketManager.API.Domain.Entities.Ticket { Title = "Problema con Login", Description = "El usuario reporta que no puede acceder al portal usando sus credenciales corporativas.", Priority = TicketManager.API.Domain.Enums.Priority.High, Status = TicketManager.API.Domain.Enums.Status.Open, CreatedBy = "evaluador@domain.com", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new TicketManager.API.Domain.Entities.Ticket { Title = "Impresora atascada", Description = "La impresora del 4to piso tiene un atasco de papel desde esta mañana.", Priority = TicketManager.API.Domain.Enums.Priority.Low, Status = TicketManager.API.Domain.Enums.Status.InProgress, CreatedBy = "empleado@domain.com", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new TicketManager.API.Domain.Entities.Ticket { Title = "Solicitud de nuevo Monitor", Description = "Necesito un monitor extra para trabajar con los excels de finanzas.", Priority = TicketManager.API.Domain.Enums.Priority.Medium, Status = TicketManager.API.Domain.Enums.Status.Resolved, CreatedBy = "finanzas@domain.com", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new TicketManager.API.Domain.Entities.Ticket { Title = "Caída de servidor principal", Description = "El servidor de correos no responde a ningún ping.", Priority = TicketManager.API.Domain.Enums.Priority.Critical, Status = TicketManager.API.Domain.Enums.Status.Open, CreatedBy = "sysadmin@domain.com", CreatedAt = DateTime.UtcNow.AddMinutes(-30) }
        );
        context.SaveChanges();
    }
}

app.Run();
