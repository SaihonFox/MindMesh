using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Removes nodes with all their connections. Undo brings back the nodes, their positions and the connections.
/// </summary>
public sealed class RemoveNodesCommand(IEnumerable<NodeId> nodes) : IGraphCommand
{
	private readonly List<NodeId> _ids = [.. nodes];
	private readonly List<(NodeInstance Node, NodePlacement Placement, bool Collapsed, NodeSize? Size)> _removedNodes = [];
	private readonly List<Connection> _removedConnections = [];

	public string Name => "Delete";

	public void Execute(GraphDocument document)
	{
		_removedNodes.Clear();
		_removedConnections.Clear();

		var idSet = _ids.ToHashSet();

		_removedConnections.AddRange(document.Graph.Connections.Where(connection =>
			idSet.Contains(connection.Source.Node) || idSet.Contains(connection.Target.Node)));

		foreach (var id in _ids)
		{
			if (document.Graph.FindNode(id) is not { } node)
				continue;

			_removedNodes.Add((node, document.Layout.GetPlacement(id), document.Layout.IsCollapsed(id), document.Layout.GetSize(id)));
		}

		foreach (var connection in _removedConnections)
			document.Graph.RemoveConnection(connection.Id);

		foreach (var (node, _, _, _) in _removedNodes)
		{
			document.Graph.RemoveNode(node.Id);
			document.Layout.Remove(node.Id);
		}
	}

	public void Undo(GraphDocument document)
	{
		foreach (var (node, placement, collapsed, size) in _removedNodes)
		{
			document.Layout.SetPlacement(node.Id, placement);
			document.Layout.SetCollapsed(node.Id, collapsed);
			document.Layout.SetSize(node.Id, size);
			document.Graph.AddNode(node);
		}

		foreach (var connection in _removedConnections)
			document.Graph.AddConnection(connection);
	}
}
