namespace UsuariosAuth.DTOs.Auth
{
    public class TokenResponse
    {
        public string accessToken { get; set; } = "";
        public DateTime accessExpiraEn { get; set; }
        public string refreshToken { get; set; } = "";
        public DateTime refreshExpiraEn { get; set; }
    }
}

