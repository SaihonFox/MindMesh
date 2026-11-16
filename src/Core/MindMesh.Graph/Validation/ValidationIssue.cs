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
}

/// <summary>
/// A problem found in a node. The code and details let the UI show a message in the user's language.
/// </summary>
/// <param name="Code">What is wrong.</param>
/// <param name="Severity">How serious it is.</param>
/// <param name="Node">The node the problem belongs to.</param>
/// <param name="Key">Key of the setting or port concerned.</param>
/// <param name="Detail">Extra value, such as the allowed minimum.</param>
public sealed record ValidationIssue(
	ValidationCode Code,
	ValidationSeverity Severity,
	NodeId Node,
	string? Key = null,
	string? Detail = null);
