namespace MindMesh.Graph.Serialization;

/// <summary>The outcome of reading a workflow file.</summary>
public sealed class WorkflowLoadResult(
	WorkflowContent? content,
	IReadOnlyList<WorkflowProblem> problems,
	int sourceFormatVersion,
	bool wasMigrated)
{
	/// <summary>The workflow, or <see langword="null"/> when the file could not be read at all.</summary>
	public WorkflowContent? Content { get; } = content;

	public IReadOnlyList<WorkflowProblem> Problems { get; } = problems;

	/// <summary>The format version the file was written in; 0 when it could not be determined.</summary>
	public int SourceFormatVersion { get; } = sourceFormatVersion;

	/// <summary>The file was written by an older version and has been upgraded in memory.</summary>
	public bool WasMigrated { get; } = wasMigrated;

	public bool Succeeded => Content is not null;

	/// <summary>
	/// Something in the file was dropped. Saving the result over the original loses it,
	/// so a caller should say so before it overwrites the file.
	/// </summary>
	public bool IsLossy => Problems.Any(problem => problem.Severity == WorkflowProblemSeverity.Error);

	public static WorkflowLoadResult Failure(WorkflowProblem problem, int sourceFormatVersion = 0) =>
		new(null, [problem], sourceFormatVersion, false);
}
