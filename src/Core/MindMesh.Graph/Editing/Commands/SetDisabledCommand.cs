using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>Disables or enables nodes. Undo restores the previous state of every node.</summary>
public sealed class SetDisabledCommand(IEnumerable<NodeId> nodes, bool isDisabled) : IGraphCommand
{
	private readonly List<NodeId> _ids = [.. nodes];
	private readonly Dictionary<NodeId, bool> _previous = [];

	public string Name => isDisabled ? "Disable" : "Enable";

	public void Execute(GraphDocument document)
	{
		_previous.Clear();

		foreach (var id in _ids)
		{
			if (document.Graph.FindNode(id) is not { } node)
				continue;

			_previous[id] = node.IsDisabled;
			document.Graph.SetDisabled(id, isDisabled);
		}
	}

	public void Undo(GraphDocument document)
	{
		foreach (var (id, wasDisabled) in _previous)
			document.Graph.SetDisabled(id, wasDisabled);
	}
}
