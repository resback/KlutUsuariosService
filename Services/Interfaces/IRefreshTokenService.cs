using UsuariosAuth.Domain.Entities;

namespace UsuariosAuth.Services.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> CrearAsync(int usuarioId, string token, DateTime expiraEn);
        Task<RefreshToken?> ObtenerVigenteAsync(int usuarioId, string token);
        Task RevocarAsync(RefreshToken rt, string? reemplazadoPor = null);
        Task RevocarTodosAsync(int usuarioId);
    }
}

