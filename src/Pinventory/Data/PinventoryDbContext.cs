using Microsoft.EntityFrameworkCore;

namespace Pinventory.Data;

public class PinventoryDbContext(DbContextOptions<PinventoryDbContext> options) : DbContext(options)
{
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.HasOne(c => c.Parent).WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => new { c.ParentId, c.Name });
        });

        modelBuilder.Entity<StorageLocation>(e =>
        {
            e.HasOne(l => l.Parent).WithMany(l => l.Children)
                .HasForeignKey(l => l.ParentId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(l => new { l.ParentId, l.Name });
        });

        modelBuilder.Entity<Part>(e =>
        {
            e.HasOne(p => p.Category).WithMany(c => c.Parts)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasOne(p => p.StorageLocation).WithMany(l => l.Parts)
                .HasForeignKey(p => p.StorageLocationId).OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(p => p.Name);
            e.Ignore(p => p.IsLowStock);
        });

        modelBuilder.Entity<Category>().HasData(SeedCategories());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Part>())
        {
            if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
            if (entry.State is EntityState.Added or EntityState.Modified) entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>Startkategorien für die Elektronik-Werkstatt. Können in der App frei geändert werden.</summary>
    private static Category[] SeedCategories()
    {
        var list = new List<Category>();
        var id = 0;

        void Add(string name, params string[] children)
        {
            var parentId = ++id;
            list.Add(new Category { Id = parentId, Name = name });
            foreach (var child in children)
                list.Add(new Category { Id = ++id, Name = child, ParentId = parentId });
        }

        Add("Passive Bauelemente", "Widerstände", "Kondensatoren", "Elkos", "Spulen & Induktivitäten", "Potentiometer & Trimmer", "Quarze & Oszillatoren");
        Add("Halbleiter", "Dioden", "Z-Dioden", "Transistoren", "MOSFETs", "Spannungsregler", "Operationsverstärker", "Logik-ICs", "Mikrocontroller");
        Add("Optoelektronik", "LEDs", "Displays", "Optokoppler", "Fotodioden & -transistoren");
        Add("Elektromechanik", "Steckverbinder", "Schalter & Taster", "Relais", "Sicherungen", "Lautsprecher & Summer");
        Add("Module & Boards", "Entwicklungsboards", "Sensormodule", "Funkmodule", "Stromversorgungsmodule");
        Add("Kabel & Draht");
        Add("Mechanik & Zubehör", "Schrauben & Abstandshalter", "Gehäuse", "Kühlkörper");
        Add("Sonstiges");

        return [.. list];
    }
}
