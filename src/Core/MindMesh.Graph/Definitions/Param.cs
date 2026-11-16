namespace MindMesh.Graph.Definitions;

/// <summary>
/// Short factories for <see cref="ParameterDefinition"/>, used when describing node types.
/// </summary>
public static class Param
{
	public static ParameterDefinition Text(
		string key,
		string name,
		string? defaultValue = null,
		bool required = false,
		string? placeholder = null,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Text,
			DefaultValue = ParameterValue.FromString(defaultValue),
			IsRequired = required,
			Placeholder = placeholder,
			Description = description,
		};

	public static ParameterDefinition Number(
		string key,
		string name,
		decimal? defaultValue = null,
		bool required = false,
		decimal? minimum = null,
		decimal? maximum = null,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Number,
			DefaultValue = defaultValue is { } number ? ParameterValue.FromNumber(number) : default,
			IsRequired = required,
			Minimum = minimum,
			Maximum = maximum,
			Description = description,
		};

	public static ParameterDefinition Boolean(
		string key,
		string name,
		bool defaultValue = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Boolean,
			DefaultValue = ParameterValue.FromBoolean(defaultValue),
			Description = description,
		};

	public static ParameterDefinition Select(
		string key,
		string name,
		string[] options,
		string? defaultValue = null,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Select,
			Options = [.. options.Select(option => new ParameterOption(option))],
			DefaultValue = ParameterValue.FromString(defaultValue ?? (options.Length > 0 ? options[0] : null)),
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition MultiSelect(
		string key,
		string name,
		string[] options,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.MultiSelect,
			Options = [.. options.Select(option => new ParameterOption(option))],
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition Code(
		string key,
		string name,
		string language,
		string? defaultValue = null,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Code,
			Language = language,
			DefaultValue = ParameterValue.FromString(defaultValue),
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition Expression(
		string key,
		string name,
		string? defaultValue = null,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Expression,
			DefaultValue = ParameterValue.FromString(defaultValue),
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition Json(
		string key,
		string name,
		string? defaultValue = null,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Json,
			Language = "json",
			DefaultValue = ParameterValue.FromString(defaultValue),
			IsRequired = required,
			Description = description,
		};

	/// <summary>
	/// A secret setting. The graph never holds the secret itself, only a reference to it.
	/// </summary>
	public static ParameterDefinition Secret(
		string key,
		string name,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.Secret,
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition File(
		string key,
		string name,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.FilePicker,
			IsRequired = required,
			Description = description,
		};

	public static ParameterDefinition Fields(
		string key,
		string name,
		bool required = false,
		string? description = null) => new()
		{
			Key = key,
			Name = name,
			Type = ParameterType.DynamicFields,
			IsRequired = required,
			Description = description,
		};
}
