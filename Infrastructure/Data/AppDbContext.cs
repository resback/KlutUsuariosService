using Microsoft.EntityFrameworkCore;
using UsuariosAuth.Domain.Entities;

namespace UsuariosAuth.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<InicioSesion> IniciosSesion => Set<InicioSesion>();
        public DbSet<BlackToken> BlackToken => Set<BlackToken>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            mb.Entity<InicioSesion>()
                .HasOne(i => i.Usuario)
                .WithMany()
                .HasForeignKey(i => i.UsuarioId);

            mb.Entity<BlackToken>()
                .HasIndex(t => t.Jti)
                .IsUnique();

            mb.Entity<RefreshToken>()
                .HasOne(r => r.Usuario)
                .WithMany()
                .HasForeignKey(r => r.UsuarioId);
        }
    }
}

