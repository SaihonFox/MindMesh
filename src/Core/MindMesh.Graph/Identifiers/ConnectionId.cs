namespace MindMesh.Graph.Identifiers;

/// <summary>
/// Stable identifier of a connection between two ports.
/// </summary>
public readonly record struct ConnectionId(Guid Value)
{
	public static ConnectionId Empty { get; } = new(Guid.Empty);

	public bool IsEmpty => Value == Guid.Empty;

	public static ConnectionId New() => new(Guid.CreateVersion7());

	public static ConnectionId Parse(string text) => new(Guid.Parse(text));

	public override string ToString() => Value.ToString("D");
}
