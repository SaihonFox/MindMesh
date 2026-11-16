using System.Globalization;
using System.Text.Json;
using MindMesh.Graph.Definitions;

namespace MindMesh.Graph.Validation;

/// <summary>
/// Checks one setting value against its definition.
/// </summary>
public static class ParameterValidator
{
	/// <summary>
	/// Returns the problem of <paramref name="value"/> as a code and an optional detail, or <c>null</c> if it is fine.
	/// </summary>
	public static (ValidationCode Code, string? Detail)? Validate(ParameterDefinition definition, ParameterValue value)
	{
		ArgumentNullException.ThrowIfNull(definition);

		if (IsEmpty(definition, value))
		{
			return definition.IsRequired ? (ValidationCode.RequiredParameterMissing, null) : null;
		}

		switch (definition.Type)
		{
			case ParameterType.Number:
				if (value.AsDecimal() is not { } number)
				{
					return (ValidationCode.InvalidNumber, null);
				}

				if (definition.Minimum is { } minimum && number < minimum)
				{
					return (ValidationCode.NumberTooSmall, minimum.ToString(CultureInfo.InvariantCulture));
				}

				if (definition.Maximum is { } maximum && number > maximum)
				{
					return (ValidationCode.NumberTooLarge, maximum.ToString(CultureInfo.InvariantCulture));
				}

				return null;

			case ParameterType.Json:
				return IsValidJson(value.AsString()) ? null : (ValidationCode.InvalidJson, null);

			case ParameterType.Select:
				return definition.Options.Count == 0
					|| definition.Options.Any(option => option.Value == value.AsString())
					? null
					: (ValidationCode.UnknownOption, value.AsString());

			default:
				return null;
		}
	}

	private static bool IsEmpty(ParameterDefinition definition, ParameterValue value)
	{
		if (value.IsNull)
		{
			return true;
		}

		return definition.Type switch
		{
			ParameterType.MultiSelect => value.AsStringList().Count == 0,
			ParameterType.DynamicFields => value.AsPairs().Count == 0,
			ParameterType.Number => false,
			ParameterType.Boolean => false,
			_ => string.IsNullOrWhiteSpace(value.AsString()),
		};
	}

	private static bool IsValidJson(string text)
	{
		try
		{
			using var document = JsonDocument.Parse(text);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}
