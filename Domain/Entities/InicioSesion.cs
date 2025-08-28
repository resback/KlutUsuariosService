namespace UsuariosAuth.Domain.Entities
{
    public class InicioSesion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public bool Exitoso { get; set; }
        public string? Ip { get; set; }
        public string? UserAgent { get; set; }
    }
}

