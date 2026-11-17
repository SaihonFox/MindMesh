using Microsoft.Extensions.Logging.Abstractions;
using MindMesh.Application.Extensions;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Serialization;
using MindMesh.Nodes.BuiltIn;

namespace MindMesh.Nodes.Tests.Serialization;

public class BuiltInWorkflowTests
{
	private static readonly ExtensionCatalog Catalog = new([new BuiltInExtension()], NullLogger<ExtensionCatalog>.Instance);

	private static WorkflowSerializer CreateSerializer() => new(Catalog.FindNode);

	[Fact]
	public void EveryBuiltInNode_SurvivesASaveAndLoadWithItsDefaults()
	{
		var content = WorkflowContent.CreateEmpty("Everything");
		var x = 0;

		foreach (var definition in Catalog.Nodes)
		{
			var node = NodeInstance.Create(definition);
			content.Graph.AddNode(node);
			content.Layout.SetPlacement(node.Id, new(x, 0));
			x += 300;
		}

		var serializer = CreateSerializer();

		var result = serializer.Deserialize(serializer.Serialize(content));

		Assert.Empty(result.Problems);
		Assert.Equal(Catalog.Nodes.Count, result.Content!.Graph.Nodes.Count);

		foreach (var node in content.Graph.Nodes)
		{
			var copy = result.Content.Graph.GetNode(node.Id);

			Assert.Equal(node.Definition.Id, copy.Definition.Id);
			Assert.Equal(node.Parameters.OrderBy(pair => pair.Key).ToArray(), copy.Parameters.OrderBy(pair => pair.Key).ToArray());
		}
	}

	[Fact]
	public void ARealisticWorkflow_KeepsItsConnections()
	{
		var content = WorkflowContent.CreateEmpty("Request then branch");
		var start = NodeInstance.Create(Catalog.Nodes.First(node => node.Inputs.Count == 0 && node.Outputs.Any(port => port.Type == PortType.Execution)));
		var request = NodeInstance.Create(Catalog.FindNode(new("http.request"))!);
		content.Graph.AddNode(start);
		content.Graph.AddNode(request);

		var from = start.Definition.Outputs.First(port => port.Type == PortType.Execution);
		var to = request.Definition.Inputs.First(port => port.Type == PortType.Execution);
		content.Graph.AddConnection(new(Graph.Identifiers.ConnectionId.New(), new PortReference(start.Id, from.Key), new PortReference(request.Id, to.Key)));
		var serializer = CreateSerializer();

		var result = serializer.Deserialize(serializer.Serialize(content));

		Assert.Empty(result.Problems);
		var connection = Assert.Single(result.Content!.Graph.Connections);
		Assert.Equal(from.Key, connection.Source.PortKey);
		Assert.Equal(to.Key, connection.Target.PortKey);
	}
}
