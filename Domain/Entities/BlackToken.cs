namespace UsuariosAuth.Domain.Entities
{
    public class BlackToken
    {
        public int Id { get; set; }
        public string Jti { get; set; } = "";
        public DateTime ExpiraEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    }
}

