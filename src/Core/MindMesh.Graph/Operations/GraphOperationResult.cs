namespace MindMesh.Graph.Operations;

/// <summary>Why an operation cannot be applied.</summary>
public enum GraphOperationProblem
{
	UnknownNodeType,
	NodeAlreadyExists,
	UnknownNode,
	UnknownPort,
	ConnectionNotAllowed,
	ConnectionNotFound,
	UnknownSetting,
	InvalidValue,
}

/// <summary>A refused operation: which one (by position in the batch) and why.</summary>
public sealed record GraphOperationFailure(int Index, GraphOperationProblem Problem, string? Detail = null);

/// <summary>The outcome of checking or applying a batch. A failed batch has changed nothing.</summary>
public sealed record GraphOperationResult(IReadOnlyList<GraphOperationFailure> Failures)
{
	public bool IsSuccess => Failures.Count == 0;
}
