namespace MindMesh.Graph.Definitions;

/// <summary>
/// Describes one setting of a node type. The inspector builds its editors from these descriptions.
/// </summary>
public sealed record ParameterDefinition
{
	public required string Key { get; init; }

	public required string Name { get; init; }

	public required ParameterType Type { get; init; }

	public string? Description { get; init; }

	public string? Placeholder { get; init; }

	public bool IsRequired { get; init; }

	public ParameterValue DefaultValue { get; init; }

	/// <summary>Choices of select settings.</summary>
	public IReadOnlyList<ParameterOption> Options { get; init; } = [];

	/// <summary>Language of code settings (<c>json</c>, <c>sql</c>, <c>javascript</c>, …); <c>null</c> for plain text.</summary>
	public string? Language { get; init; }

	public decimal? Minimum { get; init; }

	public decimal? Maximum { get; init; }

	/// <summary>The setting is shown only while this condition holds.</summary>
	public ParameterCondition? VisibleWhen { get; init; }

	/// <summary>Returns a copy that is visible only while setting <paramref name="key"/> has one of <paramref name="values"/>.</summary>
	public ParameterDefinition When(string key, params string[] values) =>
		this with { VisibleWhen = new ParameterCondition(key, values) };

	/// <summary>Evaluates <see cref="VisibleWhen"/> against the current values of a node.</summary>
	public bool IsVisible(IReadOnlyDictionary<string, ParameterValue> values)
	{
		if (VisibleWhen is not { } condition)
		{
			return true;
		}

		var current = values.TryGetValue(condition.Key, out var value) ? value.AsString() : string.Empty;

		return condition.Values.Contains(current);
	}
}
