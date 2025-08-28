using Microsoft.EntityFrameworkCore;
using UsuariosAuth.Domain.Entities;
using UsuariosAuth.Infrastructure.Data;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Services.Implementations
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly AppDbContext _db;
        public RefreshTokenService(AppDbContext db) => _db = db;

        public async Task<RefreshToken> CrearAsync(int usuarioId, string token, DateTime expiraEn)
        {
            var rt = new RefreshToken { UsuarioId = usuarioId, Token = token, ExpiraEn = expiraEn };
            _db.RefreshTokens.Add(rt);
            await _db.SaveChangesAsync();
            return rt;
        }

        public Task<RefreshToken?> ObtenerVigenteAsync(int usuarioId, string token) =>
            _db.RefreshTokens.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Token == token && x.EstaVigente);

        public async Task RevocarAsync(RefreshToken rt, string? reemplazadoPor = null)
        {
            rt.RevocadoEn = DateTime.UtcNow;
            rt.ReemplazadoPor = reemplazadoPor;
            await _db.SaveChangesAsync();
        }

        public async Task RevocarTodosAsync(int usuarioId)
        {
            var items = await _db.RefreshTokens.Where(x => x.UsuarioId == usuarioId && x.RevocadoEn == null).ToListAsync();
            foreach (var i in items) i.RevocadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}

