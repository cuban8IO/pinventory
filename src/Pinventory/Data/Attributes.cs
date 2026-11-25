using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Pinventory.Data;

/// <summary>Hilfsfunktionen für Kategorie-Eigenschaften.</summary>
public static class Attributes
{
    private static readonly CultureInfo German = CultureInfo.GetCultureInfo("de-DE");

    /// <summary>
    /// Eigenschaften, die für eine Kategorie gelten: die eigenen und die aller übergeordneten Kategorien
    /// (z.B. gilt eine Eigenschaft von "Halbleiter" auch für "Halbleiter › Dioden").
    /// </summary>
    public static List<AttributeDefinition> For(int? categoryId, IReadOnlyCollection<Category> categories,
        IReadOnlyCollection<AttributeDefinition> definitions)
    {
        if (categoryId is not int id) return [];
        var chain = Tree.SelfAndAncestors(categories, id);
        return definitions
            .Where(d => chain.Contains(d.CategoryId))
            .OrderBy(d => chain.IndexOf(d.CategoryId))
            .ThenBy(d => d.SortOrder)
            .ThenBy(d => d.Id)
            .ToList();
    }

    /// <summary>Liest eine Zahl mit Komma oder Punkt ("0,7" / "0.7").</summary>
    public static bool TryParseNumber(string? input, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        var s = input.Trim().Replace(" ", "").Replace(',', '.');
        return decimal.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>Wandelt eine Eingabe in den Speicherwert um. Null = leer, Fehlertext bei ungültiger Zahl.</summary>
    public static (string? Stored, string? Error) Normalize(AttributeDefinition def, string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, null);
        var text = input.Trim();
        if (def.Type != AttributeType.Zahl) return (text, null);
        return TryParseNumber(text, out var number)
            ? (number.ToString(CultureInfo.InvariantCulture), null)
            : (null, $"\"{def.Name}\": \"{text}\" ist keine Zahl.");
    }

    /// <summary>Gespeicherter Wert in Eingabeform (Zahlen mit Komma, ohne Einheit).</summary>
    public static string? ToInput(AttributeDefinition def, string? stored)
    {
        if (stored is null) return null;
        if (def.Type == AttributeType.Zahl && decimal.TryParse(stored, NumberStyles.Float, CultureInfo.InvariantCulture, out var n))
            return n.ToString("0.############", German);
        return stored;
    }

    /// <summary>Anzeige mit Einheit, z.B. "0,7 V".</summary>
    public static string Display(AttributeDefinition def, string? stored)
    {
        var text = ToInput(def, stored);
        if (string.IsNullOrEmpty(text)) return "";
        return string.IsNullOrEmpty(def.Unit) ? text : $"{text} {def.Unit}";
    }

    /// <summary>Sortierschlüssel: Zahlen numerisch, sonst Text.</summary>
    public static IComparable SortKey(AttributeDefinition def, string? stored) =>
        def.Type == AttributeType.Zahl
            ? (decimal.TryParse(stored, NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : decimal.MinValue)
            : (stored ?? "");

    /// <summary>Prüft alle Eingaben. Liefert die Speicherwerte (nur gefüllte) oder die erste Fehlermeldung.</summary>
    public static (Dictionary<int, string> Stored, string? Error) Validate(
        IEnumerable<AttributeDefinition> applicable, IReadOnlyDictionary<int, string?> inputs)
    {
        var stored = new Dictionary<int, string>();
        foreach (var def in applicable)
        {
            var (value, error) = Normalize(def, inputs.GetValueOrDefault(def.Id));
            if (error is not null) return ([], error);
            if (value is not null) stored[def.Id] = value;
        }
        return (stored, null);
    }

    /// <summary>Ersetzt alle Eigenschaftswerte eines Bauteils (nicht mehr passende werden entfernt).</summary>
    public static async Task ReplaceValuesAsync(PinventoryDbContext db, int partId, IReadOnlyDictionary<int, string> stored)
    {
        var existing = await db.PartAttributeValues.Where(v => v.PartId == partId).ToListAsync();
        foreach (var v in existing)
        {
            if (stored.TryGetValue(v.AttributeDefinitionId, out var newValue)) v.Value = newValue;
            else db.PartAttributeValues.Remove(v);
        }
        foreach (var (defId, value) in stored)
        {
            if (existing.All(v => v.AttributeDefinitionId != defId))
                db.PartAttributeValues.Add(new PartAttributeValue { PartId = partId, AttributeDefinitionId = defId, Value = value });
        }
        await db.SaveChangesAsync();
    }

    public static string TypeName(AttributeType type) => type switch
    {
        AttributeType.Zahl => "Zahl",
        AttributeType.Auswahl => "Auswahl",
        _ => "Text",
    };
}
