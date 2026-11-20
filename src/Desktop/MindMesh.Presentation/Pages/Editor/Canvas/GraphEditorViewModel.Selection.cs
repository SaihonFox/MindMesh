using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Presentation.Geometry;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

public sealed partial class GraphEditorViewModel
{
	private readonly List<NodeViewModel> _selected = [];
	private List<NodeViewModel> _rubberBandBase = [];

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HasSelection))]
	private ConnectionViewModel? _selectedConnection;

	/// <summary>Raised when the set of selected nodes changes.</summary>
	public event EventHandler? SelectionChanged;

	public IReadOnlyList<NodeViewModel> SelectedNodes => _selected;

	public bool HasSelection => _selected.Count > 0 || SelectedConnection is not null;

	/// <summary>
	/// Selects a node. Without <paramref name="additive"/> it becomes the only selected node;
	/// with it, it is added to the selection, or removed again if it was already selected.
	/// </summary>
	public void SelectNode(NodeViewModel node, bool additive)
	{
		if (!additive)
		{
			SetSelection([node]);
		}
		else if (node.IsSelected)
		{
			SetSelection(_selected.Where(item => item != node));
		}
		else
		{
			SetSelection([.. _selected, node]);
		}

		if (node.IsSelected)
			BringToFront(node);
	}

	public void SelectAll() => SetSelection(Nodes);

	public void ClearSelection()
	{
		SetSelection([]);
		SelectConnection(null);
	}

	/// <summary>Selects one connection (and deselects all nodes), or clears the connection selection with <c>null</c>.</summary>
	public void SelectConnection(ConnectionViewModel? connection)
	{
		if (ReferenceEquals(SelectedConnection, connection))
			return;

		if (SelectedConnection is not null)
			SelectedConnection.IsSelected = false;

		SelectedConnection = connection;

		if (connection is not null)
		{
			connection.IsSelected = true;
			SetSelection([]);
		}

		OnPropertyChanged(nameof(HasSelection));
	}

	/// <summary>Replaces the selection. Nothing is raised when the selection stays the same.</summary>
	public void SetSelection(IEnumerable<NodeViewModel> nodes)
	{
		var next = nodes.Distinct().ToList();

		if (next.Count > 0 && SelectedConnection is not null)
		{
			SelectedConnection.IsSelected = false;
			SelectedConnection = null;
		}

		if (next.Count == _selected.Count && next.All(_selected.Contains))
			return;

		foreach (var node in _selected.Where(node => !next.Contains(node)))
			node.IsSelected = false;

		foreach (var node in next)
			node.IsSelected = true;

		_selected.Clear();
		_selected.AddRange(next);

		RaiseSelectionChanged();
	}

	/// <summary>Puts a node above all others.</summary>
	public void BringToFront(NodeViewModel node) => node.ZIndex = ++_zCounter;

	/// <summary>The node under a world point, the one drawn on top when cards overlap.</summary>
	public NodeViewModel? HitTestNode(Point2D world) =>
		Nodes.Where(node => node.IsInViewport && node.Bounds.Contains(world))
			.OrderByDescending(node => node.ZIndex)
			.FirstOrDefault();

	/// <summary>
	/// The connection closest to a world point, within <paramref name="screenTolerance"/> screen units.
	/// The tolerance is converted to world units, so the click target does not shrink when the user zooms out.
	/// </summary>
	public ConnectionViewModel? HitTestConnection(Point2D world, double screenTolerance = 8)
	{
		var tolerance = screenTolerance / Viewport.Zoom;
		ConnectionViewModel? best = null;
		var bestDistance = double.MaxValue;

		foreach (var connection in Connections)
		{
			if (!connection.IsInViewport || !connection.Curve.Bounds.Inflate(tolerance).Contains(world))
				continue;

			var distance = connection.Curve.DistanceTo(world);

			if (distance <= tolerance && distance < bestDistance)
			{
				best = connection;
				bestDistance = distance;
			}
		}

		return best;
	}

	/// <summary>Removes the selected connection or the selected nodes in one undoable step.</summary>
	public void DeleteSelection()
	{
		if (SelectedConnection is { } connection)
		{
			Document.Execute(new DisconnectCommand([connection.Connection.Id]));
		}
		else if (_selected.Count > 0)
		{
			Document.Execute(new RemoveNodesCommand(_selected.Select(node => node.Id).ToList()));
		}
	}

	/// <summary>Disables the selected nodes, or enables them if all of them are disabled.</summary>
	public void ToggleDisabled()
	{
		if (_selected.Count == 0)
			return;

		var disable = _selected.Any(node => !node.IsDisabled);
		Document.Execute(new SetDisabledCommand(_selected.Select(node => node.Id).ToList(), disable));
	}

	/// <summary>Collapses the selected nodes to their header, or expands them if all of them are collapsed.</summary>
	public void ToggleCollapsed()
	{
		if (_selected.Count == 0)
			return;

		var collapse = _selected.Any(node => !node.IsCollapsed);

		foreach (var node in _selected.ToList())
			Document.Layout.SetCollapsed(node.Id, collapse);
	}

	public void BeginRubberBand(Point2D world, bool additive)
	{
		_rubberBandBase = additive ? [.. _selected] : [];

		if (!additive)
			ClearSelection();

		RubberBand.IsActive = true;
		RubberBand.Rect = new Rect2D(world.X, world.Y, 0, 0);
		_rubberBandStart = world;
	}

	public void UpdateRubberBand(Point2D world)
	{
		if (!RubberBand.IsActive)
			return;

		var rect = Rect2D.FromPoints(_rubberBandStart, world);
		RubberBand.Rect = rect;

		var hits = Nodes.Where(node => node.IsInViewport && node.Bounds.Intersects(rect));
		SetSelection(_rubberBandBase.Concat(hits));
	}

	public void EndRubberBand()
	{
		RubberBand.IsActive = false;
		_rubberBandBase = [];
	}

	private Point2D _rubberBandStart;

	private void ClearSelectionState()
	{
		foreach (var node in _selected)
			node.IsSelected = false;

		_selected.Clear();

		if (SelectedConnection is not null)
			SelectedConnection = null;

		RaiseSelectionChanged();
	}

	private void RaiseSelectionChanged()
	{
		if (_isRebuildingNode)
			return;

		OnPropertyChanged(nameof(SelectedNodes));
		OnPropertyChanged(nameof(HasSelection));
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}
}
