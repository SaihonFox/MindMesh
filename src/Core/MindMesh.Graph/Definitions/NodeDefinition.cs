using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;
using MindMesh.Graph.Validation;

namespace MindMesh.Graph.Definitions;

/// <summary>
/// The standard <see cref="INodeDefinition"/>: built with <see cref="Create"/> and a fluent builder.
/// </summary>
public sealed class NodeDefinition : INodeDefinition
{
	private readonly Dictionary<string, PortDefinition> _inputsByKey;
	private readonly Dictionary<string, PortDefinition> _outputsByKey;

	internal NodeDefinition(
		NodeDefinitionId id,
		string name,
		string description,
		IReadOnlyList<NodeCategoryId> categories,
		Version version,
		string iconKey,
		IReadOnlyList<PortDefinition> inputs,
		IReadOnlyList<PortDefinition> outputs,
		IReadOnlyList<ParameterDefinition> parameters,
		IReadOnlyList<string> keywords,
		IReadOnlyList<DynamicPortGroup> dynamicOutputs)
	{
		Id = id;
		Name = name;
		Description = description;
		Categories = categories;
		Version = version;
		IconKey = iconKey;
		Inputs = inputs;
		Outputs = outputs;
		Parameters = parameters;
		Keywords = keywords;
		DynamicOutputs = dynamicOutputs;

		_inputsByKey = inputs.ToDictionary(port => port.Key, StringComparer.Ordinal);
		_outputsByKey = outputs.ToDictionary(port => port.Key, StringComparer.Ordinal);
	}

	public NodeDefinitionId Id { get; }

	public string Name { get; }

	public string Description { get; }

	public IReadOnlyList<NodeCategoryId> Categories { get; }

	public Version Version { get; }

	public string IconKey { get; }

	public IReadOnlyList<PortDefinition> Inputs { get; }

	public IReadOnlyList<PortDefinition> Outputs { get; }

	public IReadOnlyList<ParameterDefinition> Parameters { get; }

	public IReadOnlyList<string> Keywords { get; }

	public IReadOnlyList<DynamicPortGroup> DynamicOutputs { get; }

	/// <summary>Starts the description of a node type.</summary>
	public static NodeDefinitionBuilder Create(string id, string name, NodeCategoryId category, params NodeCategoryId[] alsoIn) =>
		new(id, name, [category, .. alsoIn]);

	public PortDefinition? FindPort(string key, PortDirection direction)
	{
		if (direction == PortDirection.Input)
			return _inputsByKey.GetValueOrDefault(key);

		if (_outputsByKey.TryGetValue(key, out PortDefinition? port))
			return port;

		// A port of a dynamic group: any number up to the maximum may exist on some node of this type.
		foreach (DynamicPortGroup group in DynamicOutputs)
		{
			if (group.NumberOf(key) is { } number)
				return group.Create(number)[number - 1];
		}

		return null;
	}

	public IReadOnlyList<ValidationIssue> Validate(NodeInstance node)
	{
		ArgumentNullException.ThrowIfNull(node);

		return NodeValidator.ValidateParameters(this, node);
	}

	public override string ToString() => $"{Name} ({Id})";
}
