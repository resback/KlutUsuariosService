
namespace UsuariosAuth.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public string Token { get; set; } = "";
        public DateTime ExpiraEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime? RevocadoEn { get; set; }
        public string? ReemplazadoPor { get; set; }
        public bool EstaVigente => RevocadoEn == null && DateTime.UtcNow < ExpiraEn;
    }
}
