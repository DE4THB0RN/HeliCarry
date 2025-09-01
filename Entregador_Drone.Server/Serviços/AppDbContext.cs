using Entregador_Drone.Server.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Entregador_Drone.Server.Serviços
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.LocalizacaoCliente)
                .WithMany() // supondo que C_No não precise de coleção de pedidos
                .HasForeignKey(p => p.LocalizacaoClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Cidade> Cidade { get; set; }
        public DbSet<Drone> Drone { get; set; }

        public DbSet<Entrega> Entrega { get; set; }

        public DbSet<Pedido> Pedido { get; set; }

        public DbSet<C_No> C_No { get; set; }

    }
}
