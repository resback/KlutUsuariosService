using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UsuariosAuth.Common;
using UsuariosAuth.Infrastructure.Data;
using UsuariosAuth.Services.Implementations;
using UsuariosAuth.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuración JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// DbContext (SQL Server)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

// DI
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();


builder.Services.AddControllers().AddJsonOptions(o =>
{
    // Si deseas conservar nombres tal cual
});

// Auth JWT Bearer
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        o.Events = new JwtBearerEvents
        {
            // En esta sección se valida el token 
            OnTokenValidated = async ctx =>
            {
                var blacklist = ctx.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
                var jti = ctx.Principal?.FindFirst("jti")?.Value;
                if (!string.IsNullOrWhiteSpace(jti) && await blacklist.EstaEnListaNegraAsync(jti))
                {
                    ctx.Fail("Token en lista negra");
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
 
// Middleware de errores uniformes
app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

 

app.Run();



app.Run();
