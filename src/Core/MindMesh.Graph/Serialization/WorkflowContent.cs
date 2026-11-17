using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Serialization;

/// <summary>Everything a workflow file holds: who it is, what the graph is and where the nodes sit.</summary>
public sealed record WorkflowContent(
	Guid Id,
	string Name,
	string? Description,
	NodeGraph Graph,
	GraphLayout Layout)
{
	/// <summary>An empty workflow with a fresh identifier.</summary>
	public static WorkflowContent CreateEmpty(string name) =>
		new(Guid.CreateVersion7(), name, null, new NodeGraph(), new GraphLayout());
}
