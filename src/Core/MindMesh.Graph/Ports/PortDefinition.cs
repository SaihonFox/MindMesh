namespace MindMesh.Graph.Ports;

/// <summary>
/// Describes one input or output port of a node type.
/// </summary>
public sealed record PortDefinition
{
	public required string Key { get; init; }

	public required string Name { get; init; }

	public required PortDirection Direction { get; init; }

	public PortType Type { get; init; } = PortType.Any;

	public string? Description { get; init; }

	/// <summary>A required input must be connected for the node to be valid.</summary>
	public bool IsRequired { get; init; }

	/// <summary>
	/// True if several connections may use the port. A new connection to a port that does not allow it
	/// replaces the existing one.
	/// </summary>
	public bool AllowMultiple { get; init; }

	public PortKind Kind => PortTypes.GetKind(Type);
}
