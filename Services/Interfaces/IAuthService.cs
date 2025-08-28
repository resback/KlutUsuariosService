using Microsoft.AspNetCore.Identity.Data;
using System.Security.Claims;
using UsuariosAuth.Common;
using UsuariosAuth.DTOs.Auth;

namespace UsuariosAuth.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<object>> RegistrarAsync(RegistrarRequest dto);
        Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequestDTO dto, string? ip, string? userAgent);
        Task<ServiceResult<TokenResponse>> RefrescarAsync(RefreshRequestDTO dto);
        Task<ServiceResult<object>> LogoutAsync(LogoutRequest dto, ClaimsPrincipal user);
        object ConstruirPerfil(ClaimsPrincipal user);
    }
}
