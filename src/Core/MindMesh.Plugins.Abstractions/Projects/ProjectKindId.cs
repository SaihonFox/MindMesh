namespace MindMesh.Plugins.Abstractions.Projects;

/// <summary>
/// Identifier of a project kind, such as <c>Workflow</c>.
/// </summary>
public readonly record struct ProjectKindId(string Value)
{
	public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

	public override string ToString() => Value ?? string.Empty;
}
