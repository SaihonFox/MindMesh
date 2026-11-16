namespace MindMesh.Graph.Definitions;

/// <summary>
/// One choice of a <see cref="ParameterType.Select"/> or <see cref="ParameterType.MultiSelect"/> setting.
/// </summary>
public sealed record ParameterOption(string Value, string Label)
{
	public ParameterOption(string value)
		: this(value, value)
	{
	}
}
