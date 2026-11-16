using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Connects two ports (in either order). Connections that the new one replaces are restored by Undo.
/// </summary>
public sealed class ConnectCommand(PortReference first, PortReference second) : IGraphCommand
{
	private readonly List<Connection> _replaced = [];
	private Connection? _created;

	public string Name => "Connect";

	/// <summary>The connection created by the last <see cref="Execute"/>.</summary>
	public Connection? Created => _created;

	public void Execute(GraphDocument document)
	{
		var check = document.Graph.CheckConnection(first, second);

		if (!check.IsAllowed)
		{
			throw new InvalidOperationException($"The connection is not allowed: {check.Rejection}.");
		}

		var source = document.Graph.FindNode(first.Node)?.Definition.FindPort(first.PortKey, PortDirection.Output) is not null
			? first
			: second;
		var target = source == first ? second : first;

		_replaced.Clear();
		_replaced.AddRange(check.ReplacedConnections.Select(id => document.Graph.GetConnection(id)));

		// Keep the identifier on redo so later commands that refer to it stay valid.
		_created ??= new Connection(ConnectionId.New(), source, target);

		document.Graph.AddConnection(_created);
	}

	public void Undo(GraphDocument document)
	{
		if (_created is null)
		{
			return;
		}

		document.Graph.RemoveConnection(_created.Id);

		foreach (var replaced in _replaced)
		{
			document.Graph.AddConnection(replaced);
		}
	}
}
