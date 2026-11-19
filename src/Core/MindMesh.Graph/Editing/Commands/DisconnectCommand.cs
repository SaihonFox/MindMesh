using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Editing.Commands;

public sealed class DisconnectCommand(IEnumerable<ConnectionId> connections) : IGraphCommand
{
	private readonly List<ConnectionId> _ids = [.. connections];
	private readonly List<Connection> _removed = [];

	public string Name => "Disconnect";

	public void Execute(GraphDocument document)
	{
		_removed.Clear();

		foreach (var id in _ids)
		{
			if (document.Graph.FindConnection(id) is { } connection)
			{
				_removed.Add(connection);
				document.Graph.RemoveConnection(id);
			}
		}
	}

	public void Undo(GraphDocument document)
	{
		foreach (var connection in _removed)
			document.Graph.AddConnection(connection);
	}
}
