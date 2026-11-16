using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Ports;

public enum ConnectionRejection
{
	None,
	UnknownPort,
	SameNode,
	WrongDirection,
	TypeMismatch,
	Duplicate,
	Cycle,
}

/// <summary>
/// Result of asking whether two ports may be connected.
/// </summary>
/// <param name="Rejection"><see cref="ConnectionRejection.None"/> if the connection is allowed.</param>
/// <param name="RequiresConversion">The value is converted between two different data types.</param>
/// <param name="Replaces">Existing connections that the new one replaces (ports that accept a single connection).</param>
public sealed record ConnectionCheck(
	ConnectionRejection Rejection,
	bool RequiresConversion = false,
	IReadOnlyList<ConnectionId>? Replaces = null)
{
	public bool IsAllowed => Rejection == ConnectionRejection.None;

	public IReadOnlyList<ConnectionId> ReplacedConnections => Replaces ?? [];

	public static ConnectionCheck Rejected(ConnectionRejection reason) => new(reason);
}
