
using System.Security.Cryptography;
using System.Text;

namespace UsuariosAuth.Common
{
    public static class PasswordHasher
    {
        public static void CrearHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA256();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public static bool Verificar(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA256(salt);
            var comp = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return CryptographicOperations.FixedTimeEquals(comp, hash);
        }
    }
}
