
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UsuariosAuth.Common;
using UsuariosAuth.Domain.Entities;
using UsuariosAuth.DTOs.Auth;
using UsuariosAuth.Services.Interfaces;

namespace UsuariosAuth.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _cfg;
        private readonly SymmetricSecurityKey _key;

        public JwtService(IOptions<JwtSettings> opt)
        {
            _cfg = opt.Value;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg.Key));
        }

        public TokenResponse GenerarTokens(Usuario u)
        {
            var access = GenerarAccessToken(u, out var accExp, out _);
            var refresh = GenerarRefreshToken(out var refExp);
            return new TokenResponse
            {
                accessToken = access,
                accessExpiraEn = accExp,
                refreshToken = refresh,
                refreshExpiraEn = refExp
            };
        }

        public string GenerarAccessToken(Usuario u, out DateTime expira, out string jti)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            jti = Guid.NewGuid().ToString("N");
            expira = DateTime.UtcNow.AddMinutes(_cfg.AccessMinutes);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, u.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, u.Correo),
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim("nombre", u.Nombre)
            };
            var token = new JwtSecurityToken(_cfg.Issuer, _cfg.Audience, claims,
                         expires: expira, signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerarRefreshToken(out DateTime expira)
        {
            expira = DateTime.UtcNow.AddDays(_cfg.RefreshDays);
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }

        public ClaimsPrincipal? ValidarToken(string token, bool validarTiempo = true)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _cfg.Issuer,
                    ValidAudience = _cfg.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _key,
                    ValidateLifetime = validarTiempo,
                    ClockSkew = TimeSpan.Zero
                }, out _);
                return principal;
            }
            catch { return null; }
        }

        public string? ObtenerJti(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var tok = handler.ReadJwtToken(accessToken);
            return tok?.Id;
        }
    }
}
