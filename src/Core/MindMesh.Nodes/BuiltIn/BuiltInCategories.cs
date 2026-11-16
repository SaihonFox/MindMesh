using MindMesh.Graph.Identifiers;
using MindMesh.Plugins.Abstractions.Nodes;

namespace MindMesh.Nodes.BuiltIn;

/// <summary>
/// The node categories that ship with the application. Their identifiers double as localization key suffixes
/// (<c>Category_Triggers</c>).
/// </summary>
public static class BuiltInCategories
{
	// Workflow mode
	public static readonly NodeCategoryId Triggers = new("Triggers");
	public static readonly NodeCategoryId Logic = new("Logic");
	public static readonly NodeCategoryId Http = new("Http");
	public static readonly NodeCategoryId Database = new("Database");
	public static readonly NodeCategoryId Files = new("Files");
	public static readonly NodeCategoryId Integrations = new("Integrations");
	public static readonly NodeCategoryId DataTransformation = new("DataTransformation");

	// AI Agent mode
	public static readonly NodeCategoryId Llm = new("Llm");
	public static readonly NodeCategoryId Agent = new("Agent");
	public static readonly NodeCategoryId Memory = new("Memory");
	public static readonly NodeCategoryId Tools = new("Tools");
	public static readonly NodeCategoryId Rag = new("Rag");
	public static readonly NodeCategoryId Embeddings = new("Embeddings");
	public static readonly NodeCategoryId VectorSearch = new("VectorSearch");
	public static readonly NodeCategoryId AiContext = new("AiContext");

	// Data Flow mode
	public static readonly NodeCategoryId Input = new("Input");
	public static readonly NodeCategoryId Output = new("Output");
	public static readonly NodeCategoryId Mapping = new("Mapping");
	public static readonly NodeCategoryId Filtering = new("Filtering");
	public static readonly NodeCategoryId Aggregation = new("Aggregation");
	public static readonly NodeCategoryId Json = new("Json");
	public static readonly NodeCategoryId Csv = new("Csv");
	public static readonly NodeCategoryId Xml = new("Xml");
	public static readonly NodeCategoryId DataValidation = new("DataValidation");

	// Logic mode
	public static readonly NodeCategoryId Variables = new("Variables");
	public static readonly NodeCategoryId Conditions = new("Conditions");
	public static readonly NodeCategoryId Loops = new("Loops");
	public static readonly NodeCategoryId Functions = new("Functions");
	public static readonly NodeCategoryId Switch = new("Switch");
	public static readonly NodeCategoryId Merge = new("Merge");
	public static readonly NodeCategoryId Expressions = new("Expressions");
	public static readonly NodeCategoryId ErrorHandling = new("ErrorHandling");

	/// <summary>Every built-in category with its display name, icon and color family.</summary>
	public static IReadOnlyList<NodeCategoryDefinition> All { get; } =
	[
		new(Triggers, "Triggers", "bolt", AccentTone.Green),
		new(Logic, "Logic", "branch", AccentTone.Blue),
		new(Http, "HTTP", "globe", AccentTone.Teal),
		new(Database, "Database", "database", AccentTone.Orange),
		new(Files, "Files", "file-text", AccentTone.Yellow),
		new(Integrations, "Integrations", "plug", AccentTone.Pink),
		new(DataTransformation, "Data transformation", "shuffle", AccentTone.Orange),

		new(Llm, "LLM", "sparkles", AccentTone.Purple),
		new(Agent, "Agent", "bot", AccentTone.Purple),
		new(Memory, "Memory", "brain", AccentTone.Purple),
		new(Tools, "Tools", "wrench", AccentTone.Purple),
		new(Rag, "RAG", "book", AccentTone.Purple),
		new(Embeddings, "Embeddings", "vector", AccentTone.Purple),
		new(VectorSearch, "Vector search", "search", AccentTone.Purple),
		new(AiContext, "AI context", "message", AccentTone.Purple),

		new(Input, "Input", "download", AccentTone.Green),
		new(Output, "Output", "upload", AccentTone.Red),
		new(Mapping, "Mapping", "shuffle", AccentTone.Orange),
		new(Filtering, "Filtering", "filter", AccentTone.Orange),
		new(Aggregation, "Aggregation", "layers", AccentTone.Orange),
		new(Json, "JSON", "braces", AccentTone.Teal),
		new(Csv, "CSV", "table", AccentTone.Teal),
		new(Xml, "XML", "code", AccentTone.Teal),
		new(DataValidation, "Data validation", "check", AccentTone.Red),

		new(Variables, "Variables", "variable", AccentTone.Yellow),
		new(Conditions, "Conditions", "branch", AccentTone.Blue),
		new(Loops, "Loops", "loop", AccentTone.Blue),
		new(Functions, "Functions", "function", AccentTone.Blue),
		new(Switch, "Switch", "switch", AccentTone.Blue),
		new(Merge, "Merge", "merge", AccentTone.Blue),
		new(Expressions, "Expressions", "function", AccentTone.Pink),
		new(ErrorHandling, "Error handling", "shield", AccentTone.Red),
	];
}
