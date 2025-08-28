using Microsoft.EntityFrameworkCore;
using UsuariosAuth.Common;
using UsuariosAuth.Domain.Entities;
using UsuariosAuth.Infrastructure.Data;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Services.Implementations
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _db;
        private readonly JwtSettings _jwt;

        public UsuarioService(AppDbContext db, Microsoft.Extensions.Options.IOptions<JwtSettings> opt)
        {
            _db = db;
            _jwt = opt.Value;
        }

        public async Task<Usuario> CrearUsuarioAsync(string nombre, string correo, string password)
        {
            if (await _db.Usuarios.AnyAsync(x => x.Correo == correo))
                throw new InvalidOperationException("El correo ya está registrado");

            PasswordHasher.CrearHash(password, out var hash, out var salt);
            var u = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                PasswordHash = hash,
                PasswordSalt = salt
            };
            _db.Usuarios.Add(u);
            await _db.SaveChangesAsync();
            return u;
        }

        public async Task<(Usuario? usuario, string? motivoFalla)> AutenticarAsync(string correo, string password, string? ip, string? userAgent)
        {
            var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Correo == correo);
            if (u == null)
                return (null, "Usuario o contraseña inválidos");

            if (u.BloqueadoHasta.HasValue && u.BloqueadoHasta.Value > DateTime.UtcNow)
                return (null, "Cuenta bloqueada temporalmente");

            var ok = PasswordHasher.Verificar(password, u.PasswordHash, u.PasswordSalt);
            await RegistrarInicioSesionAsync(u.Id, ok, ip, userAgent);

            if (!ok)
            {
                u.IntentosFallidos += 1;
                if (u.IntentosFallidos >= 3)
                {
                    u.BloqueadoHasta = DateTime.UtcNow.AddMinutes(_jwt.LockMinutes);
                    u.IntentosFallidos = 0; // reinicia contador tras bloquear
                }
                await _db.SaveChangesAsync();
                return (null, "Usuario o contraseña inválidos");
            }

            // éxito
            u.IntentosFallidos = 0;
            u.BloqueadoHasta = null;
            await _db.SaveChangesAsync();
            return (u, null);
        }

        public async Task RegistrarInicioSesionAsync(int usuarioId, bool exitoso, string? ip, string? userAgent)
        {
            _db.IniciosSesion.Add(new InicioSesion
            {
                UsuarioId = usuarioId,
                Exitoso = exitoso,
                Ip = ip,
                UserAgent = userAgent
            });
            await _db.SaveChangesAsync();
        }

        public Task<Usuario?> ObtenerPorIdAsync(int id) => _db.Usuarios.FindAsync(id).AsTask();

        public Task<List<Usuario>> ListarAsync() => _db.Usuarios.AsNoTracking().ToListAsync();

        public async Task<bool> EditarNombreAsync(int id, string nombre)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return false;
            u.Nombre = nombre;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            var u = await _db.Usuarios.FindAsync(id);
            if (u == null) return false;
            _db.Usuarios.Remove(u);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

