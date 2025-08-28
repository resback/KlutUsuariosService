using Microsoft.EntityFrameworkCore;
using UsuariosAuth.Domain.Entities;
using UsuariosAuth.Infrastructure.Data;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Services.Implementations
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly AppDbContext _db;
        public TokenBlacklistService(AppDbContext db) => _db = db;

        public async Task AgregarAsync(string jti, DateTime expiraEn)
        {
            if (await _db.BlackToken.AnyAsync(t => t.Jti == jti)) return;
            _db.BlackToken.Add(new BlackToken { Jti = jti, ExpiraEn = expiraEn });
            await _db.SaveChangesAsync();
        }

        public async Task<bool> EstaEnListaNegraAsync(string jti)
        {
            var t = await _db.BlackToken.AsNoTracking().FirstOrDefaultAsync(x => x.Jti == jti);
            return t != null && t.ExpiraEn > DateTime.UtcNow;
        }
    }
}

