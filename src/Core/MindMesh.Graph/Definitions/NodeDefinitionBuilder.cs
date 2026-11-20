using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Definitions;

/// <summary>
/// Fluent description of a node type. Port and setting keys must be unique within the node.
/// </summary>
public sealed class NodeDefinitionBuilder
{
	private readonly string _id;
	private readonly string _name;
	private readonly List<NodeCategoryId> _categories;
	private readonly List<PortDefinition> _inputs = [];
	private readonly List<PortDefinition> _outputs = [];
	private readonly List<ParameterDefinition> _parameters = [];
	private readonly List<DynamicPortGroup> _dynamicOutputs = [];
	private string _description = string.Empty;
	private string _iconKey = "node";
	private string[] _keywords = [];
	private Version _version = new(1, 0);

	internal NodeDefinitionBuilder(string id, string name, List<NodeCategoryId> categories)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		ArgumentException.ThrowIfNullOrWhiteSpace(name);

		_id = id;
		_name = name;
		_categories = categories;
	}

	public NodeDefinitionBuilder Describe(string description)
	{
		_description = description;
		return this;
	}

	public NodeDefinitionBuilder Icon(string iconKey)
	{
		_iconKey = iconKey;
		return this;
	}

	public NodeDefinitionBuilder Keywords(params string[] keywords)
	{
		_keywords = keywords;
		return this;
	}

	public NodeDefinitionBuilder Version(Version version)
	{
		_version = version;
		return this;
	}

	/// <summary>
	/// Adds an input. By default execution and event inputs accept several connections and data inputs one.
	/// </summary>
	public NodeDefinitionBuilder In(
		string key,
		string name,
		PortType type = PortType.Any,
		bool required = false,
		bool? multiple = null,
		string? description = null)
	{
		_inputs.Add(new PortDefinition
		{
			Key = key,
			Name = name,
			Direction = PortDirection.Input,
			Type = type,
			IsRequired = required,
			AllowMultiple = multiple ?? PortTypes.GetKind(type) is PortKind.Execution or PortKind.Event,
			Description = description,
		});

		return this;
	}

	/// <summary>
	/// Adds an output. Outputs can feed several inputs unless <paramref name="multiple"/> is false.
	/// </summary>
	public NodeDefinitionBuilder Out(
		string key,
		string name,
		PortType type = PortType.Any,
		bool multiple = true,
		string? description = null)
	{
		_outputs.Add(new PortDefinition
		{
			Key = key,
			Name = name,
			Direction = PortDirection.Output,
			Type = type,
			AllowMultiple = multiple,
			Description = description,
		});

		return this;
	}

	/// <summary>Adds output ports whose number the user sets (the cases of a switch). The setting itself is added with <see cref="Param"/>.</summary>
	public NodeDefinitionBuilder DynamicOutputs(DynamicPortGroup group)
	{
		_dynamicOutputs.Add(group);
		return this;
	}

	public NodeDefinitionBuilder Params(IEnumerable<ParameterDefinition> parameters)
	{
		_parameters.AddRange(parameters);
		return this;
	}

	public NodeDefinitionBuilder Param(ParameterDefinition parameter)
	{
		_parameters.Add(parameter);
		return this;
	}

	public NodeDefinition Build()
	{
		var portKeys = _inputs.Concat(_outputs).Select(port => port.Key).ToList();

		if (portKeys.Count != portKeys.Distinct(StringComparer.Ordinal).Count())
			throw new InvalidOperationException($"Node '{_id}' has duplicate port keys.");

		var parameterKeys = _parameters.Select(parameter => parameter.Key).ToList();

		if (parameterKeys.Count != parameterKeys.Distinct(StringComparer.Ordinal).Count())
			throw new InvalidOperationException($"Node '{_id}' has duplicate setting keys.");

		return new NodeDefinition(
			new NodeDefinitionId(_id),
			_name,
			_description,
			_categories,
			_version,
			_iconKey,
			_inputs,
			_outputs,
			_parameters,
			_keywords,
			_dynamicOutputs);
	}
}
