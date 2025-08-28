using System.Security.Claims;
using UsuariosAuth.DTOs.Auth;
using UsuariosAuth.Domain.Entities;

namespace UsuariosAuth.Services.Interfaces
{
    public interface IJwtService
    {
        TokenResponse GenerarTokens(Usuario usuario);
        string GenerarAccessToken(Usuario usuario, out DateTime expira, out string jti);
        string GenerarRefreshToken(out DateTime expira);
        ClaimsPrincipal? ValidarToken(string token, bool validarTiempo = true);
        string? ObtenerJti(string accessToken);
    }
}

