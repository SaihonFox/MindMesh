using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Editing.Commands;

public sealed class SetTitleCommand(NodeId node, string? title) : IMergeableCommand
{
	private string? _old;
	private bool _captured;

	public string Name => "Rename";

	public NodeId Node { get; } = node;

	public string? Title { get; private set; } = title;

	public void Execute(GraphDocument document)
	{
		if (!_captured)
		{
			_old = document.Graph.GetNode(Node).Title;
			_captured = true;
		}

		document.Graph.SetTitle(Node, Title);
	}

	public void Undo(GraphDocument document) => document.Graph.SetTitle(Node, _old);

	public bool TryMerge(IGraphCommand next)
	{
		if (next is not SetTitleCommand other || other.Node != Node)
			return false;

		Title = other.Title;
		return true;
	}
}
