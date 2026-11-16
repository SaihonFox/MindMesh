namespace MindMesh.Application.Settings;

/// <summary>
/// Persisted window layout: which panels are visible and how large they are.
/// </summary>
public sealed record LayoutSettings
{
	public const double DefaultNodeLibraryWidth = 260;

	public const double DefaultInspectorWidth = 300;

	public const double DefaultAssistantWidth = 340;

	public const double DefaultConsoleHeight = 220;

	public bool IsSidebarCompact { get; init; }

	public bool IsNodeLibraryVisible { get; init; } = true;

	public bool IsInspectorVisible { get; init; } = true;

	public bool IsConsoleVisible { get; init; } = true;

	public bool IsAssistantVisible { get; init; }

	public double NodeLibraryWidth { get; init; } = DefaultNodeLibraryWidth;

	public double InspectorWidth { get; init; } = DefaultInspectorWidth;

	public double AssistantWidth { get; init; } = DefaultAssistantWidth;

	public double ConsoleHeight { get; init; } = DefaultConsoleHeight;

	/// <summary>
	/// Returns a copy with all sizes forced into their allowed ranges.
	/// </summary>
	public LayoutSettings Normalize() => this with
	{
		NodeLibraryWidth = SanitizeWidth(NodeLibraryWidth, DefaultNodeLibraryWidth),
		InspectorWidth = SanitizeWidth(InspectorWidth, DefaultInspectorWidth),
		AssistantWidth = SanitizeWidth(AssistantWidth, DefaultAssistantWidth),
		ConsoleHeight = LayoutLimits.Sanitize(
			ConsoleHeight,
			LayoutLimits.ConsoleMinHeight,
			LayoutLimits.ConsoleMaxHeight,
			DefaultConsoleHeight),
	};

	private static double SanitizeWidth(double value, double fallback) =>
		LayoutLimits.Sanitize(value, LayoutLimits.SidePanelMinWidth, LayoutLimits.SidePanelMaxWidth, fallback);
}
