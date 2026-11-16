namespace MindMesh.Plugins.Abstractions.Projects;

/// <summary>
/// Identifier of an editor mode, such as <c>Workflow</c> or <c>AiAgent</c>.
/// </summary>
public readonly record struct EditorModeId(string Value)
{
	public bool IsEmpty => string.IsNullOrWhiteSpace(Value);

	public override string ToString() => Value ?? string.Empty;
}
