using MindMesh.Graph.Definitions;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Validation;

/// <summary>
/// Finds problems in a node: invalid settings and required inputs that are not connected.
/// Whole-graph checks (reachability, a full report) arrive with the workflow core (Phase 3).
/// </summary>
public static class NodeValidator
{
	/// <summary>Checks the visible settings of a node. Disabled nodes have no problems.</summary>
	public static IReadOnlyList<ValidationIssue> ValidateParameters(INodeDefinition definition, NodeInstance node)
	{
		if (node.IsDisabled)
			return [];

		var issues = new List<ValidationIssue>();

		foreach (var parameter in definition.Parameters)
		{
			if (!parameter.IsVisible(node.Parameters))
				continue;

			if (ParameterValidator.Validate(parameter, node.GetParameter(parameter.Key)) is { } problem)
			{
				issues.Add(new ValidationIssue(
					problem.Code,
					ValidationSeverity.Error,
					node.Id,
					parameter.Key,
					problem.Detail));
			}
		}

		return issues;
	}

	/// <summary>Settings problems plus required inputs without a connection.</summary>
	public static IReadOnlyList<ValidationIssue> Validate(NodeInstance node, NodeGraph graph)
	{
		ArgumentNullException.ThrowIfNull(node);
		ArgumentNullException.ThrowIfNull(graph);

		if (node.IsDisabled)
			return [];

		var issues = new List<ValidationIssue>(node.Definition.Validate(node));

		foreach (var input in node.Definition.Inputs.Where(port => port.IsRequired))
		{
			if (graph.GetConnections(new Ports.PortReference(node.Id, input.Key)).Count == 0)
			{
				issues.Add(new ValidationIssue(
					ValidationCode.RequiredInputNotConnected,
					ValidationSeverity.Warning,
					node.Id,
					input.Key));
			}
		}

		return issues;
	}
}
