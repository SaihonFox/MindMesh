using MindMesh.Presentation.Common;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

public sealed partial class GraphEditorViewModel
{
	/// <summary>Menu for a right click on an empty part of the canvas.</summary>
	public IReadOnlyList<MenuEntry> BuildCanvasMenu(Point2D world) =>
	[
		new MenuEntry
		{
			Title = Loc["Menu_AddNode"],
			IconKey = IconKeys.Search,
			Shortcut = "Space",
			Execute = () => QuickAdd.Open(world),
		},
		MenuEntry.Separator,
		new MenuEntry
		{
			Title = Loc["Menu_Paste"],
			Shortcut = "Ctrl+V",
			IsEnabled = CanPaste,
			Execute = () => PasteAt(world),
		},
		new MenuEntry
		{
			Title = Loc["Menu_SelectAll"],
			Shortcut = "Ctrl+A",
			IsEnabled = HasNodes,
			Execute = SelectAll,
		},
		MenuEntry.Separator,
		new MenuEntry
		{
			Title = Loc["Menu_FitView"],
			Shortcut = "Ctrl+0",
			IsEnabled = HasNodes,
			Execute = () => Viewport.Fit(GetContentBounds()),
		},
	];

	/// <summary>Menu for a right click on a node. A node outside the selection becomes the selection first.</summary>
	public IReadOnlyList<MenuEntry> BuildNodeMenu(NodeViewModel node)
	{
		if (!node.IsSelected)
			SelectNode(node, additive: false);

		var allDisabled = _selected.All(item => item.IsDisabled);
		var allCollapsed = _selected.All(item => item.IsCollapsed);

		return
		[
			new MenuEntry { Title = Loc["Menu_Copy"], Shortcut = "Ctrl+C", Execute = Copy },
			new MenuEntry { Title = Loc["Menu_Duplicate"], Shortcut = "Ctrl+D", Execute = Duplicate },
			MenuEntry.Separator,
			new MenuEntry
			{
				Title = allDisabled ? Loc["Menu_Enable"] : Loc["Menu_Disable"],
				Shortcut = "Ctrl+E",
				Execute = ToggleDisabled,
			},
			new MenuEntry
			{
				Title = allCollapsed ? Loc["Menu_Expand"] : Loc["Menu_Collapse"],
				Execute = ToggleCollapsed,
			},
			MenuEntry.Separator,
			new MenuEntry { Title = Loc["Menu_Delete"], Shortcut = "Del", Execute = DeleteSelection },
		];
	}

	/// <summary>Menu for a right click on a connection.</summary>
	public IReadOnlyList<MenuEntry> BuildConnectionMenu(ConnectionViewModel connection)
	{
		SelectConnection(connection);

		return
		[
			new MenuEntry { Title = Loc["Menu_DeleteConnection"], Shortcut = "Del", Execute = DeleteSelection },
		];
	}
}
