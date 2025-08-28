using System.ComponentModel.DataAnnotations;

namespace UsuariosAuth.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        [MaxLength(120)]
        public string Nombre { get; set; } = "";

        [MaxLength(180)]
        public string Correo { get; set; } = "";

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public int IntentosFallidos { get; set; }
        public DateTime? BloqueadoHasta { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }
}

