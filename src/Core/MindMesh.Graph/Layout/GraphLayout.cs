using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Layout;

public enum LayoutChangeKind
{
	Moved,
	CollapsedChanged,
	Removed,
}

public sealed class LayoutChangedEventArgs(NodeId node, LayoutChangeKind kind) : EventArgs
{
	public NodeId Node { get; } = node;

	public LayoutChangeKind Kind { get; } = kind;
}

/// <summary>
/// Editor state of a graph: where nodes are and which are collapsed. Kept apart from the graph definition.
/// </summary>
public sealed class GraphLayout
{
	private readonly Dictionary<NodeId, NodePlacement> _placements = [];
	private readonly HashSet<NodeId> _collapsed = [];

	public event EventHandler<LayoutChangedEventArgs>? Changed;

	public NodePlacement GetPlacement(NodeId node) =>
		_placements.TryGetValue(node, out var placement) ? placement : default;

	public bool IsCollapsed(NodeId node) => _collapsed.Contains(node);

	public void SetPlacement(NodeId node, NodePlacement placement)
	{
		if (_placements.TryGetValue(node, out var current) && current == placement)
			return;

		_placements[node] = placement;
		Changed?.Invoke(this, new LayoutChangedEventArgs(node, LayoutChangeKind.Moved));
	}

	public void SetCollapsed(NodeId node, bool collapsed)
	{
		var changed = collapsed ? _collapsed.Add(node) : _collapsed.Remove(node);

		if (changed)
			Changed?.Invoke(this, new LayoutChangedEventArgs(node, LayoutChangeKind.CollapsedChanged));
	}

	/// <summary>Forgets everything about a node that was removed from the graph.</summary>
	public void Remove(NodeId node)
	{
		var removed = _placements.Remove(node);
		removed |= _collapsed.Remove(node);

		if (removed)
			Changed?.Invoke(this, new LayoutChangedEventArgs(node, LayoutChangeKind.Removed));
	}
}
