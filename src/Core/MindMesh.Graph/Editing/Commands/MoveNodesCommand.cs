using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>One node that moved from <see cref="From"/> to <see cref="To"/>.</summary>
public readonly record struct NodeMove(NodeId Node, NodePlacement From, NodePlacement To);

/// <summary>
/// Moves nodes. While the user drags, the editor already shows the moving nodes; the command is recorded
/// when the drag ends, and running it again is harmless.
/// </summary>
public sealed class MoveNodesCommand(IEnumerable<NodeMove> moves) : IGraphCommand
{
	private readonly List<NodeMove> _moves = [.. moves];

	public string Name => "Move";

	public bool IsEmpty => _moves.Count == 0;

	public void Execute(GraphDocument document)
	{
		foreach (var move in _moves)
		{
			document.Layout.SetPlacement(move.Node, move.To);
		}
	}

	public void Undo(GraphDocument document)
	{
		foreach (var move in _moves)
		{
			document.Layout.SetPlacement(move.Node, move.From);
		}
	}
}
