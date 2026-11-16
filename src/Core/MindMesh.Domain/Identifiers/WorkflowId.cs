namespace MindMesh.Domain.Identifiers;

/// <summary>
/// Stable identifier of a workflow. Backed by a time-ordered GUID (version 7).
/// </summary>
public readonly record struct WorkflowId(Guid Value)
{
	public static WorkflowId Empty { get; } = new(Guid.Empty);

	public bool IsEmpty => Value == Guid.Empty;

	public static WorkflowId New() => new(Guid.CreateVersion7());

	public static WorkflowId Parse(string text) => new(Guid.Parse(text));

	public override string ToString() => Value.ToString("D");
}
