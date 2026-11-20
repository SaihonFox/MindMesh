using Avalonia.Media;
using MindMesh.Presentation.Common;

namespace MindMesh.UI.Icons;

/// <summary>
/// Vector data of all icons. Every icon is drawn as strokes in a 24 x 24 box, in the style of outline icon sets.
/// </summary>
public static class IconCatalog
{
	private static readonly Dictionary<string, Geometry> Cache = new();

	/// <summary>Path data by icon key. Keys come from <see cref="IconKeys"/>.</summary>
	private static readonly Dictionary<string, string> AppIcons = new()
	{
		[IconKeys.Logo] = "M12 3l8 4.5v9L12 21l-8-4.5v-9zM12 3v18M4 7.5l16 9M20 7.5l-16 9",
		[IconKeys.Projects] = "M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z",
		[IconKeys.Workflows] = "M4 5h6v5H4zM14 14h6v5h-6zM10 7.5h2a2 2 0 0 1 2 2V14",
		[IconKeys.Templates] = "M4 4h16v5H4zM4 13h7v7H4zM15 13h5v7h-5z",
		[IconKeys.Marketplace] = "M5 8h14l-1 12H6zM9 8V6a3 3 0 0 1 6 0v2",
		[IconKeys.Executions] = "M21 12a9 9 0 1 1-3-6.7M21 4v5h-5M12 7.5V12l3 2",
		[IconKeys.Plugins] = "M9 3v5M15 3v5M6 8h12v4a6 6 0 0 1-12 0zM12 18v3",
		[IconKeys.Settings] = "M4 7h9M17 7h3M4 17h3M11 17h9M15 5v4M9 15v4",
		[IconKeys.Undo] = "M9 14L4 9l5-5M4 9h10.5a5.5 5.5 0 0 1 0 11H11",
		[IconKeys.Redo] = "M15 14l5-5-5-5M20 9H9.5a5.5 5.5 0 0 0 0 11H13",
		[IconKeys.AutoLayout] = "M4 4h6v6H4zM14 4h6v6h-6zM4 14h6v6H4zM14 14h6v6h-6z",
		[IconKeys.Validate] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM8 12.5l2.7 2.7L16 9.5",
		[IconKeys.Run] = "M7 4.5v15l12-7.5z",
		[IconKeys.Debug] = "M8 14a4 4 0 0 0 8 0v-3a4 4 0 0 0-8 0zM12 7V3.5M8 11H4M16 11h4M8.5 16.5L5 19M15.5 16.5L19 19M9 5.5L7.5 4M15 5.5L16.5 4",
		[IconKeys.Assistant] = "M12 3l1.8 5.2L19 10l-5.2 1.8L12 17l-1.8-5.2L5 10l5.2-1.8zM19 16l.7 1.8 1.8.7-1.8.7L19 21l-.7-1.8-1.8-.7 1.8-.7z",
		[IconKeys.PanelLeft] = "M4 4h16v16H4zM9 4v16",
		[IconKeys.PanelRight] = "M4 4h16v16H4zM15 4v16",
		[IconKeys.PanelBottom] = "M4 4h16v16H4zM4 15h16",
		[IconKeys.Terminal] = "M4 5h16v14H4zM7.5 10l3 2.5-3 2.5M13 15h4",
		[IconKeys.Search] = "M20 20l-4-4M17 11a6 6 0 1 1-12 0 6 6 0 0 1 12 0z",
		[IconKeys.Close] = "M6 6l12 12M18 6L6 18",
		[IconKeys.User] = "M16 8a4 4 0 1 1-8 0 4 4 0 0 1 8 0zM4 21a8 8 0 0 1 16 0",
		[IconKeys.Info] = "M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0zM12 11v6M12 7.5v.01",
		[IconKeys.List] = "M8 6h12M8 12h12M8 18h12M4 6h.01M4 12h.01M4 18h.01",
		[IconKeys.Alert] = "M12 3l10 18H2zM12 10v5M12 18v.01",
		[IconKeys.Braces] = "M8 4c-2 0-3 1-3 3v2c0 1.5-.8 3-2 3 1.2 0 2 1.5 2 3v2c0 2 1 3 3 3M16 4c2 0 3 1 3 3v2c0 1.5.8 3 2 3-1.2 0-2 1.5-2 3v2c0 2-1 3-3 3",
		[IconKeys.Database] = "M20 6c0 1.7-3.6 3-8 3S4 7.7 4 6s3.6-3 8-3 8 1.3 8 3zM4 6v12c0 1.7 3.6 3 8 3s8-1.3 8-3V6M4 12c0 1.7 3.6 3 8 3s8-1.3 8-3",
		[IconKeys.ChevronDown] = "M6 9l6 6 6-6",
		[IconKeys.ChevronRight] = "M9 6l6 6-6 6",
		[IconKeys.Plus] = "M12 5v14M5 12h14",
		[IconKeys.Minus] = "M5 12h14",
		[IconKeys.Fit] = "M4 9V4h5M20 9V4h-5M4 15v5h5M20 15v5h-5",
		[IconKeys.Eye] = "M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12zM15 12a3 3 0 1 1-6 0 3 3 0 0 1 6 0z",
		[IconKeys.EyeOff] = "M3 3l18 18M10.6 6.1A9 9 0 0 1 12 6c6 0 10 6 10 6a17 17 0 0 1-3 3.5M6.6 6.6A17 17 0 0 0 2 12s4 7 10 7a9 9 0 0 0 4-1M9.9 9.9a3 3 0 0 0 4.2 4.2",
		[IconKeys.Trash] = "M4 7h16M9 7V4h6v3M6 7l1 13h10l1-13M10 11v6M14 11v6",
		[IconKeys.Folder] = "M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z",
		[IconKeys.Map] = "M3 6l6-2 6 2 6-2v14l-6 2-6-2-6 2zM9 4v14M15 6v14",
		[IconKeys.Node] = "M4 5h16v14H4zM4 10h16",
		[IconKeys.Edit] = "M4 20h4L19 9l-4-4L4 16zM13.5 6.5l4 4",
		[IconKeys.Image] = "M4 5h16v14H4zM4 16l5-5 4 4 3-3 4 4M15.5 9.5v.01",
		[IconKeys.Dice] = "M5 5h14v14H5zM9 9v.01M15 9v.01M12 12v.01M9 15v.01M15 15v.01",
	};

	/// <summary>Path data by icon key: the application icons plus the icons of nodes and categories.</summary>
	public static IReadOnlyDictionary<string, string> PathData { get; } = Merge(AppIcons, NodeIcons.PathData);

	private static Dictionary<string, string> Merge(params IReadOnlyDictionary<string, string>[] sources)
	{
		var result = new Dictionary<string, string>();

		foreach (var source in sources)
		{
			foreach (var pair in source)
				result.TryAdd(pair.Key, pair.Value);
		}

		return result;
	}

	/// <summary>
	/// Returns the parsed geometry for a key, or <c>null</c> if the key is unknown.
	/// Must be called on the UI thread.
	/// </summary>
	public static Geometry? Get(string? key)
	{
		if (key is null)
			return null;

		if (Cache.TryGetValue(key, out var cached))
			return cached;

		if (!PathData.TryGetValue(key, out var data))
			return null;

		var geometry = Geometry.Parse(data);
		Cache[key] = geometry;
		return geometry;
	}
}
