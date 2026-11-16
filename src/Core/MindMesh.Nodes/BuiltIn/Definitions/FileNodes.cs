using MindMesh.Graph.Definitions;
using MindMesh.Graph.Ports;

namespace MindMesh.Nodes.BuiltIn.Definitions;

internal static class FileNodes
{
	public static IEnumerable<INodeDefinition> Create()
	{
		yield return NodeDefinition.Create("file.read", "Read File", BuiltInCategories.Files, BuiltInCategories.Input)
			.Describe("Reads a file from disk.")
			.Icon("file-text")
			.Keywords("open", "load", "disk", "path")
			.In("in", "In", PortType.Execution)
			.In("path", "Path", PortType.String)
			.Out("out", "Out", PortType.Execution)
			.Out("text", "Text", PortType.String)
			.Out("data", "Data", PortType.Binary)
			.Param(Param.File("path", "File", required: true))
			.Param(Param.Select("mode", "Read as", ["Text", "Binary"], "Text"))
			.Param(Param.Text("encoding", "Encoding", "utf-8").When("mode", "Text"))
			.Build();

		yield return NodeDefinition.Create("file.write", "Write File", BuiltInCategories.Files, BuiltInCategories.Output)
			.Describe("Writes data to a file.")
			.Icon("save")
			.Keywords("save", "disk", "export", "path")
			.In("in", "In", PortType.Execution)
			.In("content", "Content", PortType.Any, required: true)
			.Out("out", "Out", PortType.Execution)
			.Out("path", "Path", PortType.String)
			.Param(Param.File("path", "File", required: true))
			.Param(Param.Select("mode", "If the file exists", ["Overwrite", "Append", "Fail"], "Overwrite"))
			.Param(Param.Text("encoding", "Encoding", "utf-8"))
			.Build();

		yield return NodeDefinition.Create("file.parse-csv", "Parse CSV", BuiltInCategories.Files, BuiltInCategories.Csv)
			.Describe("Turns CSV text into an array of rows.")
			.Icon("table")
			.Keywords("comma", "spreadsheet", "rows", "columns")
			.In("text", "Text", PortType.String, required: true)
			.Out("rows", "Rows", PortType.Array)
			.Out("headers", "Headers", PortType.Array)
			.Param(Param.Text("delimiter", "Delimiter", ",", required: true))
			.Param(Param.Boolean("hasHeader", "First row is a header", true))
			.Param(Param.Boolean("trim", "Trim values", true))
			.Build();

		yield return NodeDefinition.Create("file.parse-json", "Parse JSON", BuiltInCategories.Files, BuiltInCategories.Json)
			.Describe("Reads a JSON file or text into structured data.")
			.Icon("braces")
			.Keywords("deserialize", "file", "decode")
			.In("text", "Text", PortType.String, required: true)
			.Out("result", "Result", PortType.Json)
			.Param(Param.Boolean("allowComments", "Allow comments"))
			.Param(Param.Boolean("allowTrailingCommas", "Allow trailing commas"))
			.Build();

		yield return NodeDefinition.Create("file.parse-xml", "Parse XML", BuiltInCategories.Files, BuiltInCategories.Xml)
			.Describe("Turns XML text into structured data.")
			.Icon("code")
			.Keywords("deserialize", "markup", "document", "xpath")
			.In("text", "Text", PortType.String, required: true)
			.Out("result", "Result", PortType.Json)
			.Param(Param.Text("xpath", "XPath filter", placeholder: "//item"))
			.Param(Param.Boolean("attributes", "Include attributes", true))
			.Build();
	}
}
