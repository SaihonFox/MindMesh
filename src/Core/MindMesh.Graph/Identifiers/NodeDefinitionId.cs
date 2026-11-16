namespace MindMesh.Graph.Identifiers;

/// <summary>
/// Identifier of a node type, for example <c>http.request</c>. Plugins use their own prefix.
/// </summary>
public readonly record struct NodeDefinitionId(string Value)
{
	public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

	public override string ToString() => Value ?? string.Empty;
}
