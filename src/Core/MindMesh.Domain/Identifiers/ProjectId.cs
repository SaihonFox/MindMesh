namespace MindMesh.Domain.Identifiers;

/// <summary>
/// Stable identifier of a project. Backed by a time-ordered GUID (version 7).
/// </summary>
public readonly record struct ProjectId(Guid Value)
{
	public static ProjectId Empty { get; } = new(Guid.Empty);

	public bool IsEmpty => Value == Guid.Empty;

	public static ProjectId New() => new(Guid.CreateVersion7());

	public static ProjectId Parse(string text) => new(Guid.Parse(text));

	public override string ToString() => Value.ToString("D");
}
