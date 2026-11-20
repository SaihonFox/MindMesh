using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;
using MindMesh.Plugins.Abstractions.Projects;
using MindMesh.Presentation.Geometry;
using MindMesh.Presentation.Pages.Editor.Library;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The search box that adds a node at a point of the canvas. When it was opened by dropping a connection on empty
/// space, only nodes that can accept that connection are offered, and the new node is connected automatically.
/// </summary>
public sealed partial class QuickAddViewModel : ObservableObject
{
	private const int MaximumResults = 40;

	private readonly GraphEditorViewModel _editor;
	private readonly IExtensionCatalog _catalog;
	private readonly NodeText _text;
	private readonly EditorWorkspace _workspace;

	[ObservableProperty]
	private bool _isOpen;

	/// <summary>The category whose nodes are shown in the side menu; its <c>IsExpanded</c> marks the highlighted row.</summary>
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasActiveCategory))]
	private LibrarySectionViewModel? _activeCategory;

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private LibraryNodeViewModel? _selectedItem;

	public QuickAddViewModel(GraphEditorViewModel editor, IExtensionCatalog catalog, NodeText text, EditorWorkspace workspace)
	{
		_editor = editor;
		_catalog = catalog;
		_text = text;
		_workspace = workspace;
	}

	/// <summary>Matches of the search text, best first.</summary>
	public ObservableCollection<LibraryNodeViewModel> Results { get; } = [];

	/// <summary>The categories offered while nothing is typed; each opens a menu with its nodes.</summary>
	public ObservableCollection<LibrarySectionViewModel> Categories { get; } = [];

	public bool IsSearching => SearchText.Trim().Length > 0;

	/// <summary>The category list is shown instead of search results while the search box is empty.</summary>
	public bool ShowCategories => !IsSearching && Categories.Count > 0;

	public bool HasActiveCategory => ActiveCategory is not null;

	/// <summary>Where the node will be added, in world coordinates.</summary>
	public Point2D Position { get; private set; }

	/// <summary>The port a dropped connection came from, if any.</summary>
	public PortViewModel? FromPort { get; private set; }

	public bool HasResults => Results.Count > 0;

	public void Open(Point2D world, PortViewModel? fromPort = null)
	{
		Position = world;
		FromPort = fromPort;
		SearchText = string.Empty;
		IsOpen = true;
		Refresh();
	}

	public void Close()
	{
		IsOpen = false;
		FromPort = null;
	}

	/// <summary>Adds the selected node (or the given one) and closes the box.</summary>
	public void Confirm(LibraryNodeViewModel? item = null)
	{
		var chosen = item ?? SelectedItem;

		if (chosen is null)
			return;

		var from = FromPort;
		var position = Position;

		Close();

		if (from is null)
		{
			_editor.AddNode(chosen.Definition, position);
		}
		else
		{
			_editor.AddNodeConnected(chosen.Definition, position, from);
		}
	}

	/// <summary>Opens the side menu of a category, closing the previous one.</summary>
	public void OpenCategory(LibrarySectionViewModel? category)
	{
		if (ReferenceEquals(category, ActiveCategory))
			return;

		if (ActiveCategory is not null)
			ActiveCategory.IsExpanded = false;

		ActiveCategory = category;

		if (category is not null)
			category.IsExpanded = true;
	}

	public void MoveSelection(int delta)
	{
		if (ShowCategories)
		{
			int current = ActiveCategory is null ? -1 : Categories.IndexOf(ActiveCategory);

			OpenCategory(Categories[Math.Clamp(current + delta, 0, Categories.Count - 1)]);
			return;
		}

		if (Results.Count == 0)
			return;

		var index = SelectedItem is null ? -1 : Results.IndexOf(SelectedItem);
		SelectedItem = Results[Math.Clamp(index + delta, 0, Results.Count - 1)];
	}

	partial void OnSearchTextChanged(string value) => Refresh();

	private bool Offers(INodeDefinition node) =>
		FromPort is null || NodeGraphPorts.FindCompatiblePort(node, FromPort.Definition) is not null;

	private void Refresh()
	{
		var query = SearchText;

		var matches = _catalog.Nodes
			.Where(node => _text.Matches(node, query))
			.Where(Offers)
			.OrderBy(node => _text.Rank(node, query))
			.ThenBy(node => _text.GetName(node), StringComparer.CurrentCultureIgnoreCase)
			.Take(MaximumResults)
			.Select(node => new LibraryNodeViewModel(node, _text))
			.ToList();

		Results.Clear();

		foreach (var match in matches)
			Results.Add(match);

		BuildCategories();

		SelectedItem = IsSearching ? Results.FirstOrDefault() : null;
		OnPropertyChanged(nameof(HasResults));
		OnPropertyChanged(nameof(IsSearching));
		OnPropertyChanged(nameof(ShowCategories));
	}

	private void BuildCategories()
	{
		OpenCategory(null);
		Categories.Clear();

		if (IsSearching)
			return;

		EditorModeDefinition? mode = _catalog.FindMode(_workspace.Mode);

		foreach (NodeCategoryId id in mode?.Categories ?? [])
		{
			if (_catalog.FindCategory(id) is not { } category)
				continue;

			List<LibraryNodeViewModel> items = _catalog.GetNodes(id)
				.Where(Offers)
				.Select(node => new LibraryNodeViewModel(node, _text))
				.OrderBy(item => item.Title, StringComparer.CurrentCultureIgnoreCase)
				.ToList();

			if (items.Count > 0)
				Categories.Add(new LibrarySectionViewModel(id, _text.GetCategoryTitle(id), category.IconKey, category.Tone, items, false));
		}
	}
}

/// <summary>
/// Picks the port a new node should connect to.
/// </summary>
public static class NodeGraphPorts
{
	/// <summary>
	/// Finds the port of <paramref name="definition"/> that can take a connection from or to <paramref name="from"/>.
	/// An exact type match wins, then a required port, then the first one declared.
	/// </summary>
	public static PortDefinition? FindCompatiblePort(INodeDefinition definition, PortDefinition from)
	{
		var candidates = from.Direction == PortDirection.Output ? definition.Inputs : definition.Outputs;

		return candidates
			.Where(port => from.Direction == PortDirection.Output
				? PortCompatibility.IsCompatible(from.Type, port.Type)
				: PortCompatibility.IsCompatible(port.Type, from.Type))
			.OrderByDescending(port => port.Type == from.Type)
			.ThenByDescending(port => port.IsRequired)
			.FirstOrDefault();
	}
}
