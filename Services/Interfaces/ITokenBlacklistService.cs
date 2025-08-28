
namespace UsuariosAuth.Services.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task AgregarAsync(string jti, DateTime expiraEn);
        Task<bool> EstaEnListaNegraAsync(string jti);
    }
}
