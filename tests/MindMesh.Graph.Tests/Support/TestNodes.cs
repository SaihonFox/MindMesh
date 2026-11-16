using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Tests.Support;

/// <summary>Small node types and helpers shared by the graph tests.</summary>
internal static class TestNodes
{
	public static readonly NodeCategoryId Category = new("Test");

	/// <summary>Has one output of several types and no inputs.</summary>
	public static NodeDefinition Producer { get; } = NodeDefinition.Create("test.producer", "Producer", Category)
		.Out("exec", "Done", PortType.Execution)
		.Out("text", "Text", PortType.String)
		.Out("integer", "Integer", PortType.Integer)
		.Out("any", "Any", PortType.Any)
		.Out("event", "Event", PortType.Event)
		.Build();

	/// <summary>Has inputs of several types and a pass-through output.</summary>
	public static NodeDefinition Consumer { get; } = NodeDefinition.Create("test.consumer", "Consumer", Category)
		.In("exec", "Run", PortType.Execution, required: true)
		.In("text", "Text", PortType.String, required: true)
		.In("number", "Number", PortType.Number)
		.In("json", "Json", PortType.Json)
		.In("many", "Many", PortType.String, multiple: true)
		.In("event", "Event", PortType.Event)
		.Out("done", "Done", PortType.Execution)
		.Out("result", "Result", PortType.String)
		.Build();

	/// <summary>Has settings of every interesting kind.</summary>
	public static NodeDefinition Configurable { get; } = NodeDefinition.Create("test.configurable", "Configurable", Category)
		.Param(Param.Text("url", "URL", required: true))
		.Param(Param.Number("count", "Count", 3, minimum: 1, maximum: 10))
		.Param(Param.Select("mode", "Mode", ["A", "B"]))
		.Param(Param.Json("body", "Body", "{}").When("mode", "B"))
		.Param(Param.Secret("token", "Token", required: true))
		.Build();

	public static GraphDocument NewDocument() => new();

	public static NodeInstance AddNode(GraphDocument document, INodeDefinition definition, double x = 0, double y = 0)
	{
		var node = NodeInstance.Create(definition);

		document.Execute(new AddNodeCommand(node, new NodePlacement(x, y)));

		return node;
	}

	public static PortReference Port(NodeInstance node, string key) => new(node.Id, key);

	public static Connection Connect(GraphDocument document, PortReference first, PortReference second)
	{
		var command = new ConnectCommand(first, second);

		document.Execute(command);

		return command.Created!;
	}
}
