using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Identifiers;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Library;

/// <summary>
/// Node library panel: the categories of the current editor mode as collapsible sections of node tiles,
/// filtered by the search text.
/// </summary>
public sealed partial class NodeLibraryViewModel : ViewModelBase
{
	private readonly EditorWorkspace _workspace;
	private readonly IExtensionCatalog _catalog;
	private readonly NodeText _text;
	private readonly GraphEditorViewModel _editor;
	private readonly Dictionary<NodeCategoryId, bool> _expanded = [];

	[ObservableProperty]
	private string _searchText = string.Empty;

	public NodeLibraryViewModel(
		ILocalizer localizer,
		EditorWorkspace workspace,
		IExtensionCatalog catalog,
		NodeText text,
		GraphEditorViewModel editor)
		: base(localizer)
	{
		_workspace = workspace;
		_catalog = catalog;
		_text = text;
		_editor = editor;

		workspace.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(EditorWorkspace.Mode))
				Refresh();
		};

		Refresh();
	}

	public ObservableCollection<LibrarySectionViewModel> Sections { get; } = [];

	public bool HasResults => Sections.Count > 0;

	public bool IsSearching => SearchText.Trim().Length > 0;

	/// <summary>Adds a node to the middle of the canvas. Dragging a tile onto the canvas is handled by the view.</summary>
	[RelayCommand]
	private void AddToCanvas(LibraryNodeViewModel? item)
	{
		if (item is not null)
			_editor.AddNodeAtViewCenter(item.Definition);
	}

	/// <summary>Adds a node at a world point of the canvas, as when a tile is dropped there.</summary>
	public void AddToCanvasAt(LibraryNodeViewModel item, Geometry.Point2D world) =>
		_editor.AddNode(item.Definition, world);

	protected override void OnLanguageChanged() => Refresh();

	partial void OnSearchTextChanged(string value) => Refresh();

	private void Refresh()
	{
		RememberExpansion();

		var searching = IsSearching;
		var query = SearchText;
		var mode = _catalog.FindMode(_workspace.Mode);
		var categories = mode?.Categories ?? [];

		Sections.Clear();

		bool first = true;

		foreach (var id in categories)
		{
			if (_catalog.FindCategory(id) is not { } category)
				continue;

			var items = _catalog.GetNodes(id)
				.Where(node => _text.Matches(node, query))
				.Select(node => new LibraryNodeViewModel(node, _text))
				.OrderBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
				.ToList();

			if (items.Count == 0)
			{
				first = false;
				continue;
			}

			// While searching every section with a match is open; otherwise the first one is open by default.
			var expanded = searching || _expanded.GetValueOrDefault(id, first);
			first = false;

			Sections.Add(new LibrarySectionViewModel(
				id,
				_text.GetCategoryTitle(id),
				category.IconKey,
				category.Tone,
				items,
				expanded));
		}

		OnPropertyChanged(nameof(HasResults));
		OnPropertyChanged(nameof(IsSearching));
	}

	private void RememberExpansion()
	{
		if (IsSearching)
			return;

		foreach (var section in Sections)
			_expanded[section.Id] = section.IsExpanded;
	}
}
