using MudBlazor;

namespace Pinventory.Components.Layout;

/// <summary>Farbschema: Platinengrün mit Kupfer-Akzent.</summary>
public static class PinventoryTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2E7D32",
            Secondary = "#C77B30",
            AppbarBackground = "#1B5E20",
            Background = "#F6F8F6",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#66BB6A",
            Secondary = "#E0954A",
            AppbarBackground = "#1E2A22",
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        },
    };
}
