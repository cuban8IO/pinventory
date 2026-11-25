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

    /// <summary>Kategorie-spezifische Eigenschaften (gelten auch für alle Unterkategorien).</summary>
    public List<AttributeDefinition> Attributes { get; set; } = [];
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

public enum MountingType
{
    SMD = 0,
    THT = 1,
    Sonstige = 2,
}

/// <summary>Bauform / Gehäuse, z.B. "0805" (SMD, Chip) oder "TO-220" (THT, TO).</summary>
public class Package
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(50)]
    public string Name { get; set; } = "";

    public MountingType Mounting { get; set; }

    /// <summary>Gruppe zum Sortieren und Filtern, z.B. "Chip", "SOT", "DIP".</summary>
    [MaxLength(50)]
    public string? Group { get; set; }

    /// <summary>z.B. Maße oder Alternativnamen ("2,0 × 1,25 mm · metrisch 2012").</summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>Reihenfolge innerhalb der Gruppe (z.B. 0402 vor 0603 vor 0805).</summary>
    public int SortOrder { get; set; }

    public List<Part> Parts { get; set; } = [];
}

public enum AttributeType
{
    Text = 0,
    Zahl = 1,
    Auswahl = 2,
}

/// <summary>Eigenschaft einer Kategorie, z.B. "Vorwärtsspannung" in V bei Dioden.</summary>
public class AttributeDefinition
{
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required(ErrorMessage = "Name ist erforderlich.")]
    [MaxLength(100)]
    public string Name { get; set; } = "";

    /// <summary>Einheit für Zahlen, z.B. "V", "mA", "mΩ".</summary>
    [MaxLength(20)]
    public string? Unit { get; set; }

    public AttributeType Type { get; set; }

    /// <summary>Auswahlmöglichkeiten, durch ";" getrennt (nur bei Typ Auswahl).</summary>
    [MaxLength(1000)]
    public string? Options { get; set; }

    public int SortOrder { get; set; }

    public List<PartAttributeValue> Values { get; set; } = [];

    public IEnumerable<string> OptionList =>
        (Options ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

/// <summary>Wert einer Eigenschaft für ein Bauteil. Zahlen werden invariant gespeichert ("0.7").</summary>
public class PartAttributeValue
{
    public int Id { get; set; }

    public int PartId { get; set; }
    public Part? Part { get; set; }

    public int AttributeDefinitionId { get; set; }
    public AttributeDefinition? AttributeDefinition { get; set; }

    [MaxLength(200)]
    public string Value { get; set; } = "";
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

    public int? PackageId { get; set; }
    public Package? Package { get; set; }

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

    public List<PartAttributeValue> Attributes { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool IsLowStock => MinQuantity > 0 && Quantity < MinQuantity;
}
