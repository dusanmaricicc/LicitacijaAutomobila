using LicitacijaAutomobila.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicitacijaAutomobila.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kategorija> Kategorije { get; set; } = null!;
        public DbSet<Automobil> Automobili { get; set; } = null!;
        public DbSet<SlikaAutomobila> SlikeAutomobila { get; set; } = null!;
        public DbSet<Ponuda> Ponude { get; set; } = null!;
        public DbSet<Notifikacija> Notifikacije { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Automobil>(entity =>
            {
                entity.HasOne(a => a.Kategorija)
                    .WithMany(k => k.Automobili)
                    .HasForeignKey(a => a.KategorijaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Prodavac)
                    .WithMany(u => u.Automobili)
                    .HasForeignKey(a => a.ProdavacId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SlikaAutomobila>(entity =>
            {
                entity.HasOne(s => s.Automobil)
                    .WithMany(a => a.Slike)
                    .HasForeignKey(s => s.AutomobilId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Ponuda>(entity =>
            {
                entity.HasOne(p => p.Automobil)
                    .WithMany(a => a.Ponude)
                    .HasForeignKey(p => p.AutomobilId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Kupac)
                    .WithMany(u => u.Ponude)
                    .HasForeignKey(p => p.KupacId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Notifikacija>(entity =>
            {
                entity.HasOne(n => n.Korisnik)
                    .WithMany(u => u.Notifikacije)
                    .HasForeignKey(n => n.KorisnikId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Automobil)
                    .WithMany()
                    .HasForeignKey(n => n.AutomobilId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
