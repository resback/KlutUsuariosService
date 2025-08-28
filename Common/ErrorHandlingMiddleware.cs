using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace UsuariosAuth.Common
{
    /// <summary>
    /// Middleware para interceptar las excepciones
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        public ErrorHandlingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext ctx)
        {
            try
            {
                await _next(ctx);
            }
            catch (Exception ex)
            {
                ctx.Response.ContentType = "application/json";
                ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var resp = ApiResponse<object>.Falla("Error interno", CodigosError.ErrorInterno,
                    new { mensaje = ex.Message });
                await ctx.Response.WriteAsync(JsonSerializer.Serialize(resp));
            }
        }
    }
}

