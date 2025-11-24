using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WarehouseApp.Models;

namespace WarehouseApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ====== Tabele domenowe ======

        public DbSet<Product> Products { get; set; }
        public DbSet<WarehouseLocation> WarehouseLocations { get; set; }
        public DbSet<StockItem> StockItems { get; set; }

        public DbSet<BusinessPartner> BusinessPartners { get; set; }
        public DbSet<WarehouseDocument> WarehouseDocuments { get; set; }
        public DbSet<WarehouseDocumentLine> WarehouseDocumentLines { get; set; }

        // ====== Konfiguracja relacji / indeksów ======

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Unikalny kod towaru
            builder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            // Dokument ma wiele linii
            builder.Entity<WarehouseDocument>()
                .HasMany(d => d.Lines)
                .WithOne(l => l.WarehouseDocument)
                .HasForeignKey(l => l.WarehouseDocumentId);

            builder.Entity<StockItem>()
        .HasIndex(s => new { s.ProductId, s.WarehouseLocationId })
        .IsUnique();
        }
    }
}



