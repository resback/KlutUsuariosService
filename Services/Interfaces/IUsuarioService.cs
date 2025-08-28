using UsuariosAuth.Domain.Entities;

namespace UsuariosAuth.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<Usuario> CrearUsuarioAsync(string nombre, string correo, string password);
        Task<(Usuario? usuario, string? motivoFalla)> AutenticarAsync(string correo, string password, string? ip, string? userAgent);
        Task RegistrarInicioSesionAsync(int usuarioId, bool exitoso, string? ip, string? userAgent);
        Task<Usuario?> ObtenerPorIdAsync(int id);
        Task<List<Usuario>> ListarAsync();
        Task<bool> EditarNombreAsync(int id, string nombre);
        Task<bool> EliminarAsync(int id);
    }
}

