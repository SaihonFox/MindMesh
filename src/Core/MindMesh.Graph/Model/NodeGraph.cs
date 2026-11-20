using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Model;

/// <summary>
/// The graph definition: nodes, their settings and the connections between them. It has no UI state
/// (positions, selection) and no runtime state. Mutations raise <see cref="Changed"/>.
/// </summary>
public sealed class NodeGraph
{
	private readonly List<NodeInstance> _nodes = [];
	private readonly Dictionary<NodeId, NodeInstance> _nodesById = [];
	private readonly List<Connection> _connections = [];
	private readonly Dictionary<ConnectionId, Connection> _connectionsById = [];
	private readonly Dictionary<PortReference, List<Connection>> _connectionsByPort = [];

	public event EventHandler<GraphChangedEventArgs>? Changed;

	public IReadOnlyList<NodeInstance> Nodes => _nodes;

	public IReadOnlyList<Connection> Connections => _connections;

	public NodeInstance? FindNode(NodeId id) => _nodesById.GetValueOrDefault(id);

	public NodeInstance GetNode(NodeId id) =>
		FindNode(id) ?? throw new KeyNotFoundException($"Node {id} is not in the graph.");

	public Connection? FindConnection(ConnectionId id) => _connectionsById.GetValueOrDefault(id);

	public Connection GetConnection(ConnectionId id) =>
		FindConnection(id) ?? throw new KeyNotFoundException($"Connection {id} is not in the graph.");

	/// <summary>Connections that use the given port.</summary>
	public IReadOnlyList<Connection> GetConnections(PortReference port) =>
		_connectionsByPort.TryGetValue(port, out var list) ? list : [];

	/// <summary>All connections that start or end at the node.</summary>
	public IReadOnlyList<Connection> GetConnections(NodeId node) =>
		[.. _connections.Where(connection => connection.Source.Node == node || connection.Target.Node == node)];

	public void AddNode(NodeInstance node)
	{
		ArgumentNullException.ThrowIfNull(node);

		if (!_nodesById.TryAdd(node.Id, node))
			throw new InvalidOperationException($"Node {node.Id} is already in the graph.");

		_nodes.Add(node);
		Raise(new NodeAdded(node));
	}

	/// <summary>Removes a node together with its connections.</summary>
	public void RemoveNode(NodeId id)
	{
		var node = GetNode(id);

		foreach (var connection in GetConnections(id))
			RemoveConnection(connection.Id);

		_nodes.Remove(node);
		_nodesById.Remove(id);
		Raise(new NodeRemoved(node));
	}

	public void AddConnection(Connection connection)
	{
		ArgumentNullException.ThrowIfNull(connection);

		var check = CheckConnection(connection.Source, connection.Target);

		if (!check.IsAllowed)
			throw new InvalidOperationException($"The connection is not allowed: {check.Rejection}.");

		// A port that accepts one connection loses the old one.
		foreach (var replaced in check.ReplacedConnections)
			RemoveConnection(replaced);

		if (!_connectionsById.TryAdd(connection.Id, connection))
			throw new InvalidOperationException($"Connection {connection.Id} is already in the graph.");

		_connections.Add(connection);
		AddToPort(connection.Source, connection);
		AddToPort(connection.Target, connection);
		Raise(new ConnectionAdded(connection));
	}

	public void RemoveConnection(ConnectionId id)
	{
		if (!_connectionsById.Remove(id, out var connection))
			throw new KeyNotFoundException($"Connection {id} is not in the graph.");

		_connections.Remove(connection);
		RemoveFromPort(connection.Source, connection);
		RemoveFromPort(connection.Target, connection);
		Raise(new ConnectionRemoved(connection));
	}

	public void SetParameter(NodeId id, string key, ParameterValue value)
	{
		var node = GetNode(id);
		var old = node.GetParameter(key);

		if (old == value)
			return;

		node.SetParameterCore(key, value);
		Raise(new ParameterChanged(id, key, old, value));
	}

	public void SetTitle(NodeId id, string? title)
	{
		var node = GetNode(id);
		var normalized = string.IsNullOrWhiteSpace(title) ? null : title;

		if (node.Title == normalized)
			return;

		var old = node.Title;
		node.Title = normalized;
		Raise(new TitleChanged(id, old, normalized));
	}

	public void SetDisabled(NodeId id, bool isDisabled)
	{
		var node = GetNode(id);

		if (node.IsDisabled == isDisabled)
			return;

		node.IsDisabled = isDisabled;
		Raise(new DisabledChanged(id, isDisabled));
	}

	/// <summary>
	/// Asks whether two ports may be connected, in either order. The answer says why not,
	/// and which existing connections a new one would replace.
	/// </summary>
	public ConnectionCheck CheckConnection(PortReference first, PortReference second)
	{
		if (FindNode(first.Node) is not { } firstNode || FindNode(second.Node) is not { } secondNode)
			return ConnectionCheck.Rejected(ConnectionRejection.UnknownPort);

		var firstPort = FindAnyPort(firstNode, first.PortKey);
		var secondPort = FindAnyPort(secondNode, second.PortKey);

		if (firstPort is null || secondPort is null)
			return ConnectionCheck.Rejected(ConnectionRejection.UnknownPort);

		if (first.Node == second.Node)
			return ConnectionCheck.Rejected(ConnectionRejection.SameNode);

		if (firstPort.Direction == secondPort.Direction)
			return ConnectionCheck.Rejected(ConnectionRejection.WrongDirection);

		var (sourceRef, sourcePort, targetRef, targetPort) = firstPort.Direction == PortDirection.Output
			? (first, firstPort, second, secondPort)
			: (second, secondPort, first, firstPort);

		if (!PortCompatibility.IsCompatible(sourcePort.Type, targetPort.Type))
			return ConnectionCheck.Rejected(ConnectionRejection.TypeMismatch);

		if (GetConnections(sourceRef).Any(connection => connection.Target == targetRef))
			return ConnectionCheck.Rejected(ConnectionRejection.Duplicate);

		if (IsReachable(targetRef.Node, sourceRef.Node))
			return ConnectionCheck.Rejected(ConnectionRejection.Cycle);

		var replaced = new List<ConnectionId>();

		if (!targetPort.AllowMultiple)
			replaced.AddRange(GetConnections(targetRef).Select(connection => connection.Id));

		if (!sourcePort.AllowMultiple)
			replaced.AddRange(GetConnections(sourceRef).Select(connection => connection.Id));

		return new ConnectionCheck(
			ConnectionRejection.None,
			PortCompatibility.RequiresConversion(sourcePort.Type, targetPort.Type),
			replaced.Count == 0 ? null : replaced.Distinct().ToList());
	}

	/// <summary>
	/// True if <paramref name="to"/> can be reached from <paramref name="from"/> by following connections
	/// from outputs to inputs. A node reaches itself.
	/// </summary>
	public bool IsReachable(NodeId from, NodeId to)
	{
		var visited = new HashSet<NodeId>();
		var stack = new Stack<NodeId>();
		stack.Push(from);

		while (stack.Count > 0)
		{
			var current = stack.Pop();

			if (current == to)
				return true;

			if (!visited.Add(current))
				continue;

			foreach (var connection in _connections)
			{
				if (connection.Source.Node == current)
					stack.Push(connection.Target.Node);
			}
		}

		return false;
	}

	private static PortDefinition? FindAnyPort(NodeInstance node, string key) =>
		node.FindPort(key, PortDirection.Input) ?? node.FindPort(key, PortDirection.Output);

	private void AddToPort(PortReference port, Connection connection)
	{
		if (!_connectionsByPort.TryGetValue(port, out var list))
		{
			list = [];
			_connectionsByPort[port] = list;
		}

		list.Add(connection);
	}

	private void RemoveFromPort(PortReference port, Connection connection)
	{
		if (!_connectionsByPort.TryGetValue(port, out var list))
			return;

		list.Remove(connection);

		if (list.Count == 0)
			_connectionsByPort.Remove(port);
	}

	private void Raise(GraphChange change) => Changed?.Invoke(this, new GraphChangedEventArgs(change));
}
