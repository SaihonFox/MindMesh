using MindMesh.Graph.Definitions;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Plugins.Abstractions.Projects;

namespace MindMesh.Plugins.Abstractions;

/// <summary>
/// The extension points available to extensions. New points are added here as the product grows.
/// </summary>
public interface IExtensionRegistrar
{
	/// <summary>Registers a kind of project (workflow, Minecraft mod, …) with its editor modes.</summary>
	void AddProjectKind(ProjectKindDefinition kind);

	/// <summary>Registers a category that groups nodes in the library.</summary>
	void AddNodeCategory(NodeCategoryDefinition category);

	/// <summary>Registers a node type.</summary>
	void AddNode(INodeDefinition node);
}
