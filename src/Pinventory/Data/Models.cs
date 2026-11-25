using System.ComponentModel.DataAnnotations;

namespace Pinventory.Data;

/// <summary>Gemeinsame Basis für hierarchische Einträge (Kategorien, Lagerorte).</summary>
public interface ITreeNode
{
    int Id { get; set; }
    string Name { get; set; }
    string? Description { get; set; }
    int? ParentId { get; set; }
}

/// <summary>Kategorie, z.B. "Passive Bauelemente › Widerstände".</summary>
public class Category : ITreeNode
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];

    public List<Part> Parts { get; set; } = [];
}

/// <summary>Lagerort, z.B. "Regal A › Sortimentskasten 2 › Fach 14".</summary>
public class StorageLocation : ITreeNode
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    [MaxLength(500)]
    public string? Description { get; set; }

    public int? ParentId { get; set; }
    public StorageLocation? Parent { get; set; }
    public List<StorageLocation> Children { get; set; } = [];

    public List<Part> Parts { get; set; } = [];
}

/// <summary>Ein Bauteil-Typ mit Bestand.</summary>
public class Part
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Elektrischer Wert, z.B. "10kΩ", "100nF", "5V".</summary>
    [MaxLength(50)]
    public string? Value { get; set; }

    /// <summary>Bauform / Gehäuse, z.B. "0805", "TO-220", "DIP-8".</summary>
    [MaxLength(50)]
    public string? Package { get; set; }

    [MaxLength(100)]
    public string? Manufacturer { get; set; }

    /// <summary>Hersteller-Teilenummer (MPN).</summary>
    [MaxLength(100)]
    public string? PartNumber { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Menge darf nicht negativ sein.")]
    public int Quantity { get; set; }

    /// <summary>Unter dieser Menge wird das Bauteil als "nachbestellen" markiert. 0 = keine Warnung.</summary>
    [Range(0, int.MaxValue, ErrorMessage = "Mindestbestand darf nicht negativ sein.")]
    public int MinQuantity { get; set; }

    [MaxLength(500)]
    [Url(ErrorMessage = "Bitte eine gültige URL angeben.")]
    public string? DatasheetUrl { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? StorageLocationId { get; set; }
    public StorageLocation? StorageLocation { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsLowStock => MinQuantity > 0 && Quantity < MinQuantity;
}
