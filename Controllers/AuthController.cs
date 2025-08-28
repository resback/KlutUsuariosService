
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UsuariosAuth.Common;
using UsuariosAuth.DTOs.Auth;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarios;
        private readonly IJwtService _jwt;
        private readonly ITokenBlacklistService _blacklist;
        private readonly IRefreshTokenService _refresh;

        public AuthController(IUsuarioService usuarios, IJwtService jwt, ITokenBlacklistService blacklist, IRefreshTokenService refresh)
        {
            _usuarios = usuarios; _jwt = jwt; _blacklist = blacklist; _refresh = refresh;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<ApiResponse<object>>> Registrar([FromBody] RegistrarRequest dto)
        {
            try
            {
                var u = await _usuarios.CrearUsuarioAsync(dto.nombre, dto.correo, dto.password);
                return Ok(ApiResponse<object>.Exito(new { id = u.Id, u.Nombre, u.Correo }, "Usuario creado"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Falla(ex.Message, CodigosError.Validacion));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Login([FromBody] LoginRequestDTO dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();

            var (u, motivo) = await _usuarios.AutenticarAsync(dto.correo, dto.password, ip, ua);
            if (u == null)
            {
                var msg = motivo == "Cuenta bloqueada temporalmente" ? CodigosError.CuentaBloqueada : CodigosError.CredencialesInvalidas;
                return Unauthorized(ApiResponse<TokenResponse>.Falla(motivo!, msg));
            }

            var access = _jwt.GenerarAccessToken(u, out var accExp, out var jti);
            var refreshTok = _jwt.GenerarRefreshToken(out var refExp);
            await _refresh.CrearAsync(u.Id, refreshTok, refExp);

            var resp = new TokenResponse
            {
                accessToken = access,
                accessExpiraEn = accExp,
                refreshToken = refreshTok,
                refreshExpiraEn = refExp
            };
            return Ok(ApiResponse<TokenResponse>.Exito(resp, "Login exitoso"));
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Refrescar([FromBody] RefreshRequestDTO dto)
        {
            var principal = HttpContext.User; // No requerido estar autenticado
            var correoClaim = principal?.FindFirst(ClaimTypes.Email)?.Value; // opcional

            // Determinar usuario a partir del refresh (guardado con usuario)
            // Por simplicidad pedimos el access token actual en Authorization? No, solo refresh.
            // Buscamos por todos los refresh vigentes (en escenario real attach usuarioId en cookie).
            // Para demo: no tenemos usuarioId aquí, asumamos que se envía también en header opcional:
            // Mejor: devolver 400 si no podemos mapear. Por prueba , aceptemos lookup por token.
            var rt = await _refresh.ObtenerVigenteAsync(0, dto.refreshToken); // 0 no filtra por usuario (simplificación)
            if (rt == null)
                return Unauthorized(ApiResponse<TokenResponse>.Falla("Refresh token inválido o expirado", CodigosError.TokenInvalido));

            var usuarioId = rt.UsuarioId;
            var u = await _usuarios.ObtenerPorIdAsync(usuarioId);
            if (u == null)
                return Unauthorized(ApiResponse<TokenResponse>.Falla("Usuario no encontrado", CodigosError.RecursoNoEncontrado));

            // Rotación de refresh
            await _refresh.RevocarAsync(rt, reemplazadoPor: "rotado");
            var nuevoRefresh = _jwt.GenerarRefreshToken(out var refExp);
            await _refresh.CrearAsync(usuarioId, nuevoRefresh, refExp);

            var access = _jwt.GenerarAccessToken(u, out var accExp, out _);
            var resp = new TokenResponse
            {
                accessToken = access,
                accessExpiraEn = accExp,
                refreshToken = nuevoRefresh,
                refreshExpiraEn = refExp
            };
            return Ok(ApiResponse<TokenResponse>.Exito(resp, "Token renovado"));
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest dto)
        {
            var jti = _jwt.ObtenerJti(dto.accessToken);
            if (string.IsNullOrWhiteSpace(jti))
                return BadRequest(ApiResponse<object>.Falla("Token inválido", CodigosError.TokenInvalido));

            var principal = _jwt.ValidarToken(dto.accessToken, validarTiempo: false);
            if (principal == null)
                return BadRequest(ApiResponse<object>.Falla("Token no verificable", CodigosError.TokenInvalido));

            var exp = principal.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
            var expUnix = exp != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime : DateTime.UtcNow.AddMinutes(15);
            await _blacklist.AgregarAsync(jti, expUnix);

            // Opcional: revocar todos los refresh del usuario que cierra sesión
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId > 0) await _refresh.RevocarTodosAsync(userId);

            return Ok(ApiResponse<object>.Exito(null, "Sesión cerrada"));
        }

        [Authorize]
        [HttpGet("yo")]
        public ActionResult<ApiResponse<object>> Yo()
        {
            var sub = User.FindFirst("sub")?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
            var nombre = User.FindFirst("nombre")?.Value;
            return Ok(ApiResponse<object>.Exito(new { id = sub, correo = email, nombre }, "Perfil"));
        }
    }
}
