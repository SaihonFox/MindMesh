namespace MindMesh.Graph.Identifiers;

/// <summary>
/// Stable identifier of a node in a graph. Backed by a time-ordered GUID (version 7).
/// </summary>
public readonly record struct NodeId(Guid Value)
{
	public static NodeId Empty { get; } = new(Guid.Empty);

	public bool IsEmpty => Value == Guid.Empty;

	public static NodeId New() => new(Guid.CreateVersion7());

	public static NodeId Parse(string text) => new(Guid.Parse(text));

	public override string ToString() => Value.ToString("D");
}
