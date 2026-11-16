using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Editing;

/// <summary>
/// A copy of part of a graph (nodes, their settings, positions and the connections between them)
/// that can be pasted again and again. Secret settings are not copied.
/// </summary>
public sealed class GraphFragment
{
	private readonly List<Entry> _entries;
	private readonly List<(NodeId Source, string SourcePort, NodeId Target, string TargetPort)> _connections;

	private GraphFragment(
		List<Entry> entries,
		List<(NodeId Source, string SourcePort, NodeId Target, string TargetPort)> connections)
	{
		_entries = entries;
		_connections = connections;
	}

	private sealed record Entry(NodeId OriginalId, NodeInstance Template, NodePlacement Placement, bool Collapsed);

	public bool IsEmpty => _entries.Count == 0;

	public int NodeCount => _entries.Count;

	/// <summary>Copies the given nodes of a document. Unknown identifiers are ignored.</summary>
	public static GraphFragment Capture(GraphDocument document, IEnumerable<NodeId> nodes)
	{
		ArgumentNullException.ThrowIfNull(document);

		var ids = nodes.ToHashSet();
		var entries = new List<Entry>();

		foreach (var node in document.Graph.Nodes.Where(node => ids.Contains(node.Id)))
		{
			entries.Add(new Entry(
				node.Id,
				node.CloneWithNewId(),
				document.Layout.GetPlacement(node.Id),
				document.Layout.IsCollapsed(node.Id)));
		}

		var captured = entries.Select(entry => entry.OriginalId).ToHashSet();
		var connections = document.Graph.Connections
			.Where(connection => captured.Contains(connection.Source.Node) && captured.Contains(connection.Target.Node))
			.Select(connection => (connection.Source.Node, connection.Source.PortKey, connection.Target.Node, connection.Target.PortKey))
			.ToList();

		return new GraphFragment(entries, connections);
	}

	/// <summary>
	/// Builds one undoable command that adds copies of the nodes, moved by the given offset.
	/// </summary>
	/// <param name="offsetX">Horizontal shift of the copies.</param>
	/// <param name="offsetY">Vertical shift of the copies.</param>
	/// <param name="createdNodes">Identifiers of the new nodes, in the order of the original nodes.</param>
	public CompositeCommand CreatePasteCommand(double offsetX, double offsetY, out IReadOnlyList<NodeId> createdNodes)
	{
		var map = new Dictionary<NodeId, NodeId>();
		var commands = new List<IGraphCommand>();

		foreach (var entry in _entries)
		{
			var copy = entry.Template.CloneWithNewId();
			map[entry.OriginalId] = copy.Id;

			commands.Add(new AddNodeCommand(
				copy,
				new NodePlacement(entry.Placement.X + offsetX, entry.Placement.Y + offsetY),
				entry.Collapsed));
		}

		foreach (var (source, sourcePort, target, targetPort) in _connections)
		{
			commands.Add(new ConnectCommand(
				new PortReference(map[source], sourcePort),
				new PortReference(map[target], targetPort)));
		}

		createdNodes = [.. _entries.Select(entry => map[entry.OriginalId])];

		return new CompositeCommand("Paste", commands);
	}

	/// <summary>The top-left corner of the copied nodes.</summary>
	public NodePlacement TopLeft => _entries.Count == 0
		? default
		: new NodePlacement(_entries.Min(entry => entry.Placement.X), _entries.Min(entry => entry.Placement.Y));
}
