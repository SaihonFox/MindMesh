using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Validation;

namespace MindMesh.Graph.Definitions;

/// <summary>
/// Describes a node type: what it is called, which ports it has and which settings it offers.
/// Definitions know nothing about the UI. Executors are added by the runtime (Phase 4).
/// </summary>
public interface INodeDefinition
{
	NodeDefinitionId Id { get; }

	string Name { get; }

	string Description { get; }

	/// <summary>Categories the node is listed under. The first one is the primary category (color and icon).</summary>
	IReadOnlyList<NodeCategoryId> Categories { get; }

	Version Version { get; }

	/// <summary>Key of the node icon; the UI maps it to a picture.</summary>
	string IconKey { get; }

	IReadOnlyList<PortDefinition> Inputs { get; }

	IReadOnlyList<PortDefinition> Outputs { get; }

	/// <summary>The configuration schema: the inspector is built from it.</summary>
	IReadOnlyList<ParameterDefinition> Parameters { get; }

	/// <summary>Extra words the node is found by in searches.</summary>
	IReadOnlyList<string> Keywords { get; }

	/// <summary>Finds a port of the given direction by key.</summary>
	PortDefinition? FindPort(string key, PortDirection direction);

	/// <summary>Checks the settings of a node of this type.</summary>
	IReadOnlyList<ValidationIssue> Validate(NodeInstance node);
}
