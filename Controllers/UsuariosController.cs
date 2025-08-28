
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsuariosAuth.Common;
using UsuariosAuth.DTOs.Usuarios;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _svc;
        public UsuariosController(IUsuarioService svc) { _svc = svc; }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDto>>>> Listar()
        {
            var items = await _svc.ListarAsync();
            var dtos = items.Select(x => new UsuarioDto { id = x.Id, nombre = x.Nombre, correo = x.Correo });
            return Ok(ApiResponse<IEnumerable<UsuarioDto>>.Exito(dtos, "Listado"));
        }

        [HttpPost]
        [AllowAnonymous]  
        public async Task<ActionResult<ApiResponse<object>>> Crear([FromBody] UsuarioCrearDto dto)
        {
            try
            {
                var u = await _svc.CrearUsuarioAsync(dto.nombre, dto.correo, dto.password);
                return Ok(ApiResponse<object>.Exito(new { id = u.Id }, "Creado"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Falla(ex.Message, CodigosError.Validacion));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Editar(int id, [FromBody] UsuarioEditarDto dto)
        {
            var ok = await _svc.EditarNombreAsync(id, dto.nombre);
            if (!ok) return NotFound(ApiResponse<object>.Falla("No encontrado", CodigosError.RecursoNoEncontrado));
            return Ok(ApiResponse<object>.Exito(null, "Actualizado"));
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<object>>> Eliminar(int id)
        {
            var ok = await _svc.EliminarAsync(id);
            if (!ok) return NotFound(ApiResponse<object>.Falla("No encontrado", CodigosError.RecursoNoEncontrado));
            return Ok(ApiResponse<object>.Exito(null, "Eliminado"));
        }
    }
}
