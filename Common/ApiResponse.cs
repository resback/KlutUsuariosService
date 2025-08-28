namespace UsuariosAuth.Common
{
    /// <summary>
    /// Respuestas estandarizadas en formato JSON
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ApiResponse<T>
    {
        public bool ok { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public string? codigo { get; set; }
        public T? datos { get; set; }
        public object? error { get; set; }

        public static ApiResponse<T> Exito(T? datos, string mensaje = "OK", string? codigo = null)
            => new ApiResponse<T>
            {
                ok = true,
                mensaje = mensaje,
                codigo = codigo,
                datos = datos
            };

        public static ApiResponse<T> Falla(string mensaje, string? codigo = null, object? error = null)
            => new ApiResponse<T>
            {
                ok = false,
                mensaje = mensaje,
                codigo = codigo,
                error = error
            };
    }
}
