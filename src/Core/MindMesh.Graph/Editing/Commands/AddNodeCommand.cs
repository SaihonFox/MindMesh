using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Editing.Commands;

public sealed class AddNodeCommand(NodeInstance node, NodePlacement placement, bool collapsed = false) : IGraphCommand
{
	public string Name => "Add node";

	public NodeInstance Node { get; } = node;

	public void Execute(GraphDocument document)
	{
		document.Layout.SetPlacement(Node.Id, placement);
		document.Layout.SetCollapsed(Node.Id, collapsed);
		document.Graph.AddNode(Node);
	}

	public void Undo(GraphDocument document)
	{
		document.Graph.RemoveNode(Node.Id);
		document.Layout.Remove(Node.Id);
	}
}
