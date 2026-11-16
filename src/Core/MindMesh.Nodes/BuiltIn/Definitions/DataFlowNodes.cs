using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

/// <summary>
/// Nodes that give the Data Flow mode its entry and exit points, grouping and checking.
/// </summary>
internal static class DataFlowNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("flow.input", "Workflow Input", BuiltInCategories.Input)
			.Describe("Declares a value the workflow receives from its caller.")
			.Icon("download")
			.Keywords("parameter", "argument", "entry", "start")
			.Out("value", "Value", PortType.Any)
			.Param(Param.Text("name", "Name", required: true, placeholder: "customerId"))
			.Param(Param.Select("type", "Type", ["Any", "String", "Number", "Boolean", "Object", "Array"], "Any"))
			.Param(Param.Json("default", "Default value", "null"))
			.Param(Param.Boolean("required", "Required", true))
			.Build();

		yield return NodeDefinition.Create("flow.output", "Workflow Output", BuiltInCategories.Output)
			.Describe("Declares a value the workflow returns to its caller.")
			.Icon("upload")
			.Keywords("result", "return", "exit", "end")
			.In("value", "Value", PortType.Any, required: true)
			.Param(Param.Text("name", "Name", "result", required: true))
			.Build();

		yield return NodeDefinition.Create("flow.aggregate", "Aggregate", BuiltInCategories.Aggregation)
			.Describe("Summarizes an array: count, sum, average, minimum or maximum.")
			.Icon("layers")
			.Keywords("sum", "count", "average", "group", "reduce", "total")
			.In("items", "Items", PortType.Array, required: true)
			.Out("result", "Result", PortType.Any)
			.Param(Param.Select("operation", "Operation", ["Count", "Sum", "Average", "Minimum", "Maximum"], "Count"))
			.Param(Param.Expression("field", "Value of each item", "item").When("operation", "Sum", "Average", "Minimum", "Maximum"))
			.Param(Param.Expression("groupBy", "Group by"))
			.Build();

		yield return NodeDefinition.Create("flow.validate", "Validate Data", BuiltInCategories.DataValidation)
			.Describe("Checks data against a JSON schema and routes valid and invalid data separately.")
			.Icon("check")
			.Keywords("schema", "verify", "assert", "check")
			.In("in", "In", PortType.Json, required: true)
			.Out("valid", "Valid", PortType.Json)
			.Out("invalid", "Invalid", PortType.Json)
			.Out("errors", "Errors", PortType.Array)
			.Param(Param.Json("schema", "JSON schema", "{\n  \"type\": \"object\"\n}", required: true))
			.Param(Param.Boolean("stopOnInvalid", "Stop when invalid"))
			.Build();
	}
}
