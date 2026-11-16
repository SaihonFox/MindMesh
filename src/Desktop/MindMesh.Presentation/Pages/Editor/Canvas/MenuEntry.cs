namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// One line of a context menu. The view only turns entries into menu items; what they do is decided here.
/// </summary>
public sealed record MenuEntry
{
	public string Title { get; init; } = string.Empty;

	public string? IconKey { get; init; }

	/// <summary>Shortcut shown on the right, such as <c>Ctrl+C</c>.</summary>
	public string? Shortcut { get; init; }

	public bool IsEnabled { get; init; } = true;

	public bool IsSeparator { get; init; }

	public Action? Execute { get; init; }

	public static MenuEntry Separator { get; } = new() { IsSeparator = true };
}
