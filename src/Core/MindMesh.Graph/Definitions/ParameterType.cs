namespace MindMesh.Graph.Definitions;

/// <summary>
/// Kind of editor the inspector builds for a node setting.
/// </summary>
public enum ParameterType
{
	Text,
	Number,
	Boolean,
	Select,
	MultiSelect,
	Code,
	Expression,
	Json,
	Secret,
	FilePicker,

	/// <summary>A list of name/value pairs (headers, query parameters, field mappings).</summary>
	DynamicFields,
}
