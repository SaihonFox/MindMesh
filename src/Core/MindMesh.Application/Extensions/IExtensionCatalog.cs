using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Plugins.Abstractions;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Application.Extensions;

/// <summary>
/// Everything extensions have registered: project kinds, editor modes, node categories and node types.
/// The catalog is read-only after startup.
/// </summary>
public interface IExtensionCatalog
{
	IReadOnlyList<ExtensionInfo> Extensions { get; }

	IReadOnlyList<ProjectKindDefinition> ProjectKinds { get; }

	IReadOnlyList<NodeCategoryDefinition> Categories { get; }

	IReadOnlyList<INodeDefinition> Nodes { get; }

	/// <summary>Registrations that were rejected, for diagnostics. Empty when everything is consistent.</summary>
	IReadOnlyList<ExtensionProblem> Problems { get; }

	ProjectKindDefinition? FindProjectKind(ProjectKindId id);

	/// <summary>Finds an editor mode in any project kind.</summary>
	EditorModeDefinition? FindMode(EditorModeId id);

	NodeCategoryDefinition? FindCategory(NodeCategoryId id);

	INodeDefinition? FindNode(NodeDefinitionId id);

	/// <summary>The nodes that belong to a category, in registration order.</summary>
	IReadOnlyList<INodeDefinition> GetNodes(NodeCategoryId category);
}
