using MindMesh.Graph.Definitions;
using MindMesh.Nodes.BuiltIn.Definitions;
using MindMesh.Plugins.Abstractions;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Nodes.BuiltIn;

/// <summary>
/// The content that ships with MindMesh: the Workflow project kind, its editor modes, categories and nodes.
/// It registers through the same contract as any plugin.
/// </summary>
public sealed class BuiltInExtension : IMindMeshExtension
{
	public static readonly ProjectKindId WorkflowKind = new("Workflow");

	public static readonly EditorModeId WorkflowMode = new("Workflow");

	public static readonly EditorModeId AiAgentMode = new("AiAgent");

	public static readonly EditorModeId DataFlowMode = new("DataFlow");

	public static readonly EditorModeId LogicMode = new("Logic");

	public ExtensionInfo Info { get; } = new("mindmesh.builtin", "MindMesh built-in", new Version(1, 0), "MindMesh", IsBuiltIn: true);

	public void Register(IExtensionRegistrar registrar)
	{
		ArgumentNullException.ThrowIfNull(registrar);

		foreach (var category in BuiltInCategories.All)
			registrar.AddNodeCategory(category);

		foreach (var node in CreateNodes())
			registrar.AddNode(node);

		registrar.AddProjectKind(CreateWorkflowKind());
	}

	/// <summary>All built-in node definitions.</summary>
	public static IReadOnlyList<INodeDefinition> CreateNodes() =>
	[
		.. TriggerNodes.Create(),
		.. LogicNodes.Create(),
		.. HttpNodes.Create(),
		.. DatabaseNodes.Create(),
		.. DataNodes.Create(),
		.. DataFlowNodes.Create(),
		.. AiNodes.Create(),
		.. FileNodes.Create(),
		.. IntegrationNodes.Create(),
	];

	private static ProjectKindDefinition CreateWorkflowKind() => new(
		WorkflowKind,
		"Workflow",
		"Automations and AI agents built from connected nodes.",
		"workflow",
		[
			new EditorModeDefinition(
				WorkflowMode,
				"Workflow",
				[
					BuiltInCategories.Triggers,
					BuiltInCategories.Logic,
					BuiltInCategories.Http,
					BuiltInCategories.Database,
					BuiltInCategories.Files,
					BuiltInCategories.Integrations,
					BuiltInCategories.DataTransformation,
				]),
			new EditorModeDefinition(
				AiAgentMode,
				"AI Agent",
				[
					BuiltInCategories.Llm,
					BuiltInCategories.Agent,
					BuiltInCategories.Memory,
					BuiltInCategories.Tools,
					BuiltInCategories.Rag,
					BuiltInCategories.Embeddings,
					BuiltInCategories.VectorSearch,
					BuiltInCategories.AiContext,
				]),
			new EditorModeDefinition(
				DataFlowMode,
				"Data Flow",
				[
					BuiltInCategories.Input,
					BuiltInCategories.Output,
					BuiltInCategories.Mapping,
					BuiltInCategories.Filtering,
					BuiltInCategories.Aggregation,
					BuiltInCategories.Json,
					BuiltInCategories.Csv,
					BuiltInCategories.Xml,
					BuiltInCategories.DataValidation,
				]),
			new EditorModeDefinition(
				LogicMode,
				"Logic",
				[
					BuiltInCategories.Variables,
					BuiltInCategories.Conditions,
					BuiltInCategories.Loops,
					BuiltInCategories.Functions,
					BuiltInCategories.Switch,
					BuiltInCategories.Merge,
					BuiltInCategories.Expressions,
					BuiltInCategories.ErrorHandling,
				]),
		]);
}
