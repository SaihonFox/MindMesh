namespace MindMesh.Graph.Identifiers;

/// <summary>
/// Identifier of a node category, for example <c>Triggers</c>. Categories are data, so extensions can add their own.
/// </summary>
public readonly record struct NodeCategoryId(string Value)
{
	public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

	public override string ToString() => Value ?? string.Empty;
}
