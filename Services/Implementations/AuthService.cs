using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using UsuariosAuth.Common;
using UsuariosAuth.DTOs.Auth;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioService _usuarios;
        private readonly IJwtService _jwt;
        private readonly ITokenBlacklistService _blacklist;
        private readonly IRefreshTokenService _refresh;

        public AuthService(IUsuarioService usuarios, IJwtService jwt, ITokenBlacklistService blacklist, IRefreshTokenService refresh)
        {
            _usuarios = usuarios;
            _jwt = jwt;
            _blacklist = blacklist;
            _refresh = refresh;
        }

        public async Task<ServiceResult<object>> RegistrarAsync(RegistrarRequest dto)
        {
            try
            {
                var u = await _usuarios.CrearUsuarioAsync(dto.nombre, dto.correo, dto.password);
                var resp = ApiResponse<object>.Exito(new { id = u.Id, u.Nombre, u.Correo }, "Usuario creado");
                return ServiceResult<object>.De(resp, StatusCodes.Status200OK);
            }
            catch (InvalidOperationException ex)
            {
                var resp = ApiResponse<object>.Falla(ex.Message, CodigosError.Validacion);
                return ServiceResult<object>.De(resp, StatusCodes.Status400BadRequest);
            }
        }

        public async Task<ServiceResult<TokenResponse>> LoginAsync(LoginRequestDTO dto, string? ip, string? userAgent)
        {
            var (u, motivo) = await _usuarios.AutenticarAsync(dto.correo, dto.password, ip, userAgent);
            if (u == null)
            {
                var code = motivo == "Cuenta bloqueada temporalmente" ? CodigosError.CuentaBloqueada : CodigosError.CredencialesInvalidas;
                var respFail = ApiResponse<TokenResponse>.Falla(motivo!, code);
                return ServiceResult<TokenResponse>.De(respFail, StatusCodes.Status401Unauthorized);
            }

            var access = _jwt.GenerarAccessToken(u, out var accExp, out _);
            var refreshTok = _jwt.GenerarRefreshToken(out var refExp);
            await _refresh.CrearAsync(u.Id, refreshTok, refExp);

            var payload = new TokenResponse
            {
                accessToken = access,
                accessExpiraEn = accExp,
                refreshToken = refreshTok,
                refreshExpiraEn = refExp
            };
            return ServiceResult<TokenResponse>.De(ApiResponse<TokenResponse>.Exito(payload, "Login exitoso"),
                                                  StatusCodes.Status200OK);
        }

        public async Task<ServiceResult<TokenResponse>> RefrescarAsync(RefreshRequestDTO dto)
        {
            var rt = await _refresh.ObtenerVigentePorTokenAsync(dto.refreshToken);
            if (rt == null)
            {
                var resp = ApiResponse<TokenResponse>.Falla("Refresh token inválido o expirado", CodigosError.TokenInvalido);
                return ServiceResult<TokenResponse>.De(resp, StatusCodes.Status401Unauthorized);
            }

            var u = await _usuarios.ObtenerPorIdAsync(rt.UsuarioId);
            if (u == null)
            {
                var resp = ApiResponse<TokenResponse>.Falla("Usuario no encontrado", CodigosError.RecursoNoEncontrado);
                return ServiceResult<TokenResponse>.De(resp, StatusCodes.Status404NotFound);
            }

            // Rotación de refresh
            await _refresh.RevocarAsync(rt, reemplazadoPor: "rotado");
            var nuevoRefresh = _jwt.GenerarRefreshToken(out var refExp);
            await _refresh.CrearAsync(u.Id, nuevoRefresh, refExp);

            var access = _jwt.GenerarAccessToken(u, out var accExp, out _);
            var data = new TokenResponse
            {
                accessToken = access,
                accessExpiraEn = accExp,
                refreshToken = nuevoRefresh,
                refreshExpiraEn = refExp
            };
            return ServiceResult<TokenResponse>.De(ApiResponse<TokenResponse>.Exito(data, "Token renovado"),
                                                   StatusCodes.Status200OK);
        }

        public async Task<ServiceResult<object>> LogoutAsync(LogoutRequest dto, ClaimsPrincipal user)
        {
            var jti = _jwt.ObtenerJti(dto.accessToken);
            if (string.IsNullOrWhiteSpace(jti))
                return ServiceResult<object>.De(ApiResponse<object>.Falla("Token inválido", CodigosError.TokenInvalido),
                                                StatusCodes.Status400BadRequest);

            // Leer exp del token sin validar vigencia
            DateTime expira;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var tk = handler.ReadJwtToken(dto.accessToken);
                var expClaim = tk.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                expira = expClaim != null
                    ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime
                    : DateTime.UtcNow.AddMinutes(15);
            }
            catch
            {
                expira = DateTime.UtcNow.AddMinutes(15);
            }

            await _blacklist.AgregarAsync(jti, expira);

            // Revocar refresh del usuario autenticado
            var sub = user.FindFirst("sub")?.Value;
            if (int.TryParse(sub, out var userId) && userId > 0)
                await _refresh.RevocarTodosAsync(userId);

            return ServiceResult<object>.De(ApiResponse<object>.Exito(null, "Sesión cerrada"),
                                            StatusCodes.Status200OK);
        }

        public object ConstruirPerfil(ClaimsPrincipal user)
        {
            var sub = user.FindFirst("sub")?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
            var nombre = user.FindFirst("nombre")?.Value;
            return new { id = sub, correo = email, nombre };
        }
    }
}
