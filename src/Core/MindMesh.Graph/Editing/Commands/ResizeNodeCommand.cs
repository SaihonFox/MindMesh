using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Changes the size of one node card. While the user drags the handle the editor already shows the new size;
/// the command is recorded when the drag ends, and running it again is harmless.
/// </summary>
public sealed class ResizeNodeCommand(NodeId node, NodeSize? from, NodeSize? to) : IGraphCommand
{
	public string Name => "Resize";

	public bool IsEmpty => from == to;

	public void Execute(GraphDocument document) => document.Layout.SetSize(node, to);

	public void Undo(GraphDocument document) => document.Layout.SetSize(node, from);
}
