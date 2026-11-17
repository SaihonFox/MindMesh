namespace MindMesh.Graph.Serialization;

public enum WorkflowProblemSeverity
{
	/// <summary>Everything was loaded; something is worth knowing.</summary>
	Warning,

	/// <summary>Part of the file could not be loaded and is missing from the result.</summary>
	Error,

	/// <summary>The file could not be loaded at all.</summary>
	Fatal,
}

public enum WorkflowProblemCode
{
	FileTooLarge,
	NotJson,
	NotAWorkflow,
	MissingFormatVersion,
	NewerFormat,
	MigrationFailed,
	InvalidWorkflowId,
	InvalidNodeId,
	DuplicateNodeId,
	UnknownNodeType,
	NodeVersionNewer,
	UnknownParameter,
	InvalidConnection,
	DuplicateConnectionId,
	ConnectionRejected,
	ConnectionDropped,
}

/// <summary>Something found while reading a workflow file.</summary>
/// <param name="Subject">What it is about: a node or connection id, or a field name.</param>
/// <param name="Detail">Extra text, such as the node type or the reason a connection was refused.</param>
public sealed record WorkflowProblem(
	WorkflowProblemCode Code,
	WorkflowProblemSeverity Severity,
	string? Subject = null,
	string? Detail = null);
