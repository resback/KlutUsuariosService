
namespace UsuariosAuth.DTOs.Auth
{
    public class RegistrarRequest
    {
        public string nombre { get; set; } = "";
        public string correo { get; set; } = "";
        public string password { get; set; } = "";
    }
}
