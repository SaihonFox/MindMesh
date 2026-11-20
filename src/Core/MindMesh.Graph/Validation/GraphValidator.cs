using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Validation;

/// <summary>
/// Checks a whole graph: the problems of every node plus the rules that need the whole picture
/// (a trigger exists, nodes are connected, there is no loop).
/// </summary>
public static class GraphValidator
{
	/// <summary>The category whose nodes start a workflow.</summary>
	public static NodeCategoryId TriggerCategory { get; } = new("Triggers");

	/// <summary>Collects every problem of the graph. An empty graph has none.</summary>
	public static IReadOnlyList<ValidationIssue> Validate(NodeGraph graph)
	{
		ArgumentNullException.ThrowIfNull(graph);

		List<ValidationIssue> issues = [.. graph.Nodes.SelectMany(node => NodeValidator.Validate(node, graph))];

		if (graph.Nodes.Count == 0)
			return issues;

		if (!graph.Nodes.Any(node => !node.IsDisabled && node.Definition.Categories.Contains(TriggerCategory)))
			issues.Add(new ValidationIssue(ValidationCode.GraphHasNoTrigger, ValidationSeverity.Warning, NodeId.Empty));

		if (graph.Nodes.Count > 1)
		{
			foreach (NodeInstance node in graph.Nodes.Where(node => !node.IsDisabled && graph.GetConnections(node.Id).Count == 0))
				issues.Add(new ValidationIssue(ValidationCode.NodeNotConnected, ValidationSeverity.Warning, node.Id));
		}

		foreach (NodeId node in FindNodesInCycles(graph))
			issues.Add(new ValidationIssue(ValidationCode.GraphHasCycle, ValidationSeverity.Error, node));

		return issues;
	}

	/// <summary>True when no issue is an error. Warnings do not block anything.</summary>
	public static bool IsRunnable(IEnumerable<ValidationIssue> issues) =>
		!issues.Any(issue => issue.Severity == ValidationSeverity.Error);

	/// <summary>The nodes that cannot be put in an order because they lie on a loop or after one (Kahn's algorithm).</summary>
	private static IReadOnlyList<NodeId> FindNodesInCycles(NodeGraph graph)
	{
		Dictionary<NodeId, int> incoming = graph.Nodes.ToDictionary(node => node.Id, _ => 0);

		foreach (Connection connection in graph.Connections)
			incoming[connection.Target.Node]++;

		Queue<NodeId> ready = new(incoming.Where(pair => pair.Value == 0).Select(pair => pair.Key));
		int ordered = 0;

		while (ready.Count > 0)
		{
			NodeId current = ready.Dequeue();

			ordered++;

			foreach (Connection connection in graph.Connections.Where(connection => connection.Source.Node == current))
			{
				if (--incoming[connection.Target.Node] == 0)
					ready.Enqueue(connection.Target.Node);
			}
		}

		return ordered == graph.Nodes.Count ? [] : [.. incoming.Where(pair => pair.Value > 0).Select(pair => pair.Key)];
	}
}
