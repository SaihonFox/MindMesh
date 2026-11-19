using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;
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

	[ObservableProperty]
	private bool _isOpen;

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private LibraryNodeViewModel? _selectedItem;

	public QuickAddViewModel(GraphEditorViewModel editor, IExtensionCatalog catalog, NodeText text)
	{
		_editor = editor;
		_catalog = catalog;
		_text = text;
	}

	public ObservableCollection<LibraryNodeViewModel> Results { get; } = [];

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

	public void MoveSelection(int delta)
	{
		if (Results.Count == 0)
			return;

		var index = SelectedItem is null ? -1 : Results.IndexOf(SelectedItem);
		SelectedItem = Results[Math.Clamp(index + delta, 0, Results.Count - 1)];
	}

	partial void OnSearchTextChanged(string value) => Refresh();

	private void Refresh()
	{
		var query = SearchText;

		var matches = _catalog.Nodes
			.Where(node => _text.Matches(node, query))
			.Where(node => FromPort is null || NodeGraphPorts.FindCompatiblePort(node, FromPort.Definition) is not null)
			.OrderBy(node => _text.Rank(node, query))
			.ThenBy(node => _text.GetName(node), StringComparer.CurrentCultureIgnoreCase)
			.Take(MaximumResults)
			.Select(node => new LibraryNodeViewModel(node, _text))
			.ToList();

		Results.Clear();

		foreach (var match in matches)
			Results.Add(match);

		SelectedItem = Results.FirstOrDefault();
		OnPropertyChanged(nameof(HasResults));
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
