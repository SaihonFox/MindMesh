using Avalonia.Styling;

namespace MindMesh.UI.Themes;

/// <summary>The custom theme variants. Each one falls back to the light or dark theme for values it does not define.</summary>
public static class AppThemeVariants
{
	public static ThemeVariant Midnight { get; } = new("Midnight", ThemeVariant.Dark);

	public static ThemeVariant Forest { get; } = new("Forest", ThemeVariant.Dark);

	public static ThemeVariant Ocean { get; } = new("Ocean", ThemeVariant.Light);

	public static ThemeVariant Rose { get; } = new("Rose", ThemeVariant.Light);
}
