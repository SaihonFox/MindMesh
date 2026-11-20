using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Validation;

public enum ValidationSeverity
{
	Warning,
	Error,
}

public enum ValidationCode
{
	RequiredParameterMissing,
	InvalidNumber,
	NumberTooSmall,
	NumberTooLarge,
	InvalidJson,
	UnknownOption,
	RequiredInputNotConnected,

	/// <summary>The graph has nodes but no enabled trigger, so nothing can start it. A graph-level issue.</summary>
	GraphHasNoTrigger,

	/// <summary>A node has no connection at all although the graph has other nodes.</summary>
	NodeNotConnected,

	/// <summary>The connections form a loop. Connecting through the editor refuses loops, but a file can contain one.</summary>
	GraphHasCycle,
}

/// <summary>
/// A problem found in a node. The code and details let the UI show a message in the user's language.
/// </summary>
/// <param name="Code">What is wrong.</param>
/// <param name="Severity">How serious it is.</param>
/// <param name="Node">The node the problem belongs to; <see cref="NodeId.Empty"/> for a problem of the whole graph.</param>
/// <param name="Key">Key of the setting or port concerned.</param>
/// <param name="Detail">Extra value, such as the allowed minimum.</param>
public sealed record ValidationIssue(
	ValidationCode Code,
	ValidationSeverity Severity,
	NodeId Node,
	string? Key = null,
	string? Detail = null)
{
	/// <summary>True when the problem concerns the whole graph and no single node.</summary>
	public bool IsGraphLevel => Node.IsEmpty;
}
