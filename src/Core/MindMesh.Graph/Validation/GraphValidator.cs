using MindMesh.Graph.Model;

namespace MindMesh.Graph.Validation;

public static class GraphValidator
{
	/// <summary>Collects the problems of every node of the graph.</summary>
	public static IReadOnlyList<ValidationIssue> Validate(NodeGraph graph)
	{
		ArgumentNullException.ThrowIfNull(graph);

		return [.. graph.Nodes.SelectMany(node => NodeValidator.Validate(node, graph))];
	}
}
