using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Model;

/// <summary>
/// A node placed in a graph: its type, user title and settings. Position and collapsed state are editor state
/// and live in <see cref="Layout.GraphLayout"/>.
/// </summary>
public sealed class NodeInstance
{
	private readonly Dictionary<string, ParameterValue> _parameters = new(StringComparer.Ordinal);

	public NodeInstance(
		NodeId id,
		INodeDefinition definition,
		string? title = null,
		bool isDisabled = false,
		IEnumerable<KeyValuePair<string, ParameterValue>>? parameters = null)
	{
		ArgumentNullException.ThrowIfNull(definition);

		Id = id;
		Definition = definition;
		Title = title;
		IsDisabled = isDisabled;

		if (parameters is null)
		{
			foreach (var parameter in definition.Parameters.Where(item => !item.DefaultValue.IsNull))
			{
				_parameters[parameter.Key] = parameter.DefaultValue;
			}
		}
		else
		{
			foreach (var (key, value) in parameters.Where(item => !item.Value.IsNull))
			{
				_parameters[key] = value;
			}
		}
	}

	public NodeId Id { get; }

	public INodeDefinition Definition { get; }

	/// <summary>The user's own title, or <c>null</c> to use the name of the node type.</summary>
	public string? Title { get; internal set; }

	public bool IsDisabled { get; internal set; }

	public IReadOnlyDictionary<string, ParameterValue> Parameters => _parameters;

	public string DisplayTitle => string.IsNullOrWhiteSpace(Title) ? Definition.Name : Title;

	/// <summary>Creates a node of the given type with default settings and a new identifier.</summary>
	public static NodeInstance Create(INodeDefinition definition) => new(NodeId.New(), definition);

	public ParameterValue GetParameter(string key) =>
		_parameters.TryGetValue(key, out var value) ? value : default;

	internal void SetParameterCore(string key, ParameterValue value)
	{
		if (value.IsNull)
		{
			_parameters.Remove(key);
		}
		else
		{
			_parameters[key] = value;
		}
	}

	/// <summary>
	/// Copies the node under a new identifier. Secret settings are not copied: a secret belongs to one node.
	/// </summary>
	public NodeInstance CloneWithNewId()
	{
		var copied = Parameters.Where(pair => !IsSecret(pair.Key));

		return new NodeInstance(NodeId.New(), Definition, Title, IsDisabled, copied);
	}

	private bool IsSecret(string key) =>
		Definition.Parameters.Any(parameter => parameter.Key == key && parameter.Type == ParameterType.Secret);

	public override string ToString() => $"{DisplayTitle} [{Id}]";
}
