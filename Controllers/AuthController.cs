using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UsuariosAuth.Common;
using UsuariosAuth.DTOs.Auth;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Controllers
{
    [ApiController]
    [Route("api/auth/v1")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("registrar")]
        public async Task<ActionResult<ApiResponse<object>>> Registrar([FromBody] RegistrarRequest dto)
        {
            var r = await _auth.RegistrarAsync(dto);
            return StatusCode(r.statusCode, r.respuesta);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Login([FromBody] LoginRequestDTO dto)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();
            var r = await _auth.LoginAsync(dto, ip, ua);
            return StatusCode(r.statusCode, r.respuesta);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<TokenResponse>>> Refrescar([FromBody] RefreshRequestDTO dto)
        {
            var r = await _auth.RefrescarAsync(dto);
            return StatusCode(r.statusCode, r.respuesta);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest dto)
        {
            var r = await _auth.LogoutAsync(dto, User);
            return StatusCode(r.statusCode, r.respuesta);
        }

        [Authorize]
        [HttpGet("Profile")]
        public ActionResult<ApiResponse<object>> Profile()
        {
            var perfil = _auth.ConstruirPerfil(User);
            return Ok(ApiResponse<object>.Exito(perfil, "Perfil"));
        }
    }
}
