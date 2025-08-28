using Microsoft.AspNetCore.Http;

namespace UsuariosAuth.Common
{
    public class ServiceResult<T>
    {
        public ApiResponse<T> respuesta { get; set; } = null!;
        public int statusCode { get; set; } = StatusCodes.Status200OK;

        public static ServiceResult<T> De(ApiResponse<T> r, int code)
            => new() { respuesta = r, statusCode = code };
    }
}
