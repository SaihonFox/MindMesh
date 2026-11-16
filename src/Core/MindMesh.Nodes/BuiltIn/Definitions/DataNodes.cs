using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class DataNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("data.set-variable", "Set Variable", BuiltInCategories.DataTransformation, BuiltInCategories.Variables)
			.Describe("Stores a value in a workflow variable.")
			.Icon("variable")
			.Keywords("assign", "store", "let", "state")
			.In("in", "In", PortType.Execution)
			.In("value", "Value", PortType.Any, description: "Overrides the value setting.")
			.Out("out", "Out", PortType.Execution)
			.Out("stored", "Value", PortType.Any)
			.Param(Param.Text("name", "Variable name", required: true, placeholder: "counter"))
			.Param(Param.Expression("value", "Value", required: false))
			.Param(Param.Select("scope", "Scope", ["Workflow", "Run"], "Run"))
			.Build();

		yield return NodeDefinition.Create("data.get-variable", "Get Variable", BuiltInCategories.DataTransformation, BuiltInCategories.Variables)
			.Describe("Reads the value of a workflow variable.")
			.Icon("variable")
			.Keywords("read", "load", "state")
			.Out("value", "Value", PortType.Any)
			.Param(Param.Text("name", "Variable name", required: true, placeholder: "counter"))
			.Param(Param.Json("fallback", "Value when missing", "null"))
			.Build();

		yield return NodeDefinition.Create("data.json-parse", "JSON Parse", BuiltInCategories.DataTransformation, BuiltInCategories.Json)
			.Describe("Turns JSON text into structured data.")
			.Icon("braces")
			.Keywords("deserialize", "decode", "text to object")
			.In("text", "Text", PortType.String, required: true)
			.Out("result", "Result", PortType.Json)
			.Out("failed", "On error", PortType.Event)
			.Param(Param.Boolean("failOnError", "Fail on invalid JSON", true))
			.Build();

		yield return NodeDefinition.Create("data.json-serialize", "JSON Serialize", BuiltInCategories.DataTransformation, BuiltInCategories.Json)
			.Describe("Turns structured data into JSON text.")
			.Icon("braces")
			.Keywords("serialize", "encode", "stringify", "object to text")
			.In("value", "Value", PortType.Any, required: true)
			.Out("text", "Text", PortType.String)
			.Param(Param.Boolean("indent", "Indent the result"))
			.Param(Param.Boolean("camelCase", "Use camelCase names"))
			.Build();

		yield return NodeDefinition.Create("data.mapping", "Data Mapping", BuiltInCategories.DataTransformation, BuiltInCategories.Mapping)
			.Describe("Builds a new object from fields of the input using expressions.")
			.Icon("shuffle")
			.Keywords("map", "fields", "rename", "reshape")
			.In("in", "In", PortType.Object, required: true)
			.Out("out", "Out", PortType.Object)
			.Param(Param.Fields("fields", "Fields", description: "Name on the left, expression on the right."))
			.Param(Param.Boolean("keepOthers", "Keep unmapped fields"))
			.Build();

		yield return NodeDefinition.Create("data.filter", "Filter", BuiltInCategories.DataTransformation, BuiltInCategories.Filtering)
			.Describe("Keeps only the items of an array that match a condition.")
			.Icon("filter")
			.Keywords("where", "select", "match", "remove")
			.In("items", "Items", PortType.Array, required: true)
			.Out("matched", "Matched", PortType.Array)
			.Out("rejected", "Rejected", PortType.Array)
			.Param(Param.Expression("condition", "Condition", "item != null", required: true))
			.Build();

		yield return NodeDefinition.Create("data.transform", "Transform", BuiltInCategories.DataTransformation, BuiltInCategories.Mapping)
			.Describe("Changes the shape of data with a script.")
			.Icon("wand")
			.Keywords("script", "convert", "javascript", "code")
			.In("in", "In", PortType.Any, required: true)
			.Out("out", "Out", PortType.Any)
			.Param(Param.Select("language", "Language", ["JavaScript", "C#"], "JavaScript"))
			.Param(Param.Code("code", "Code", "javascript", "return input;", required: true))
			.Build();

		yield return NodeDefinition.Create("data.expression", "Expression", BuiltInCategories.DataTransformation, BuiltInCategories.Expressions)
			.Describe("Computes a value from an expression.")
			.Icon("function")
			.Keywords("formula", "calculate", "evaluate", "math")
			.In("in", "In", PortType.Any)
			.Out("result", "Result", PortType.Any)
			.Param(Param.Expression("expression", "Expression", "input", required: true))
			.Build();

		yield return NodeDefinition.Create("data.merge-objects", "Merge Objects", BuiltInCategories.DataTransformation, BuiltInCategories.Merge)
			.Describe("Combines several objects into one.")
			.Icon("merge")
			.Keywords("combine", "join", "extend", "assign")
			.In("a", "Object A", PortType.Object, required: true)
			.In("b", "Object B", PortType.Object, required: true)
			.Out("result", "Result", PortType.Object)
			.Param(Param.Select("conflict", "When a field exists in both", ["Use B", "Use A", "Fail"], "Use B"))
			.Param(Param.Boolean("deep", "Merge nested objects", true))
			.Build();
	}
}
