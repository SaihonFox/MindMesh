using System.Buffers;
using System.Globalization;
using System.Text.Json;

namespace MindMesh.Graph.Definitions;

/// <summary>
/// Value of a node setting, stored as JSON text so any kind of setting is saved the same way.
/// <c>default</c> is the empty value.
/// </summary>
public readonly record struct ParameterValue
{
	private readonly string? _json;

	public ParameterValue(string? json)
	{
		_json = string.IsNullOrWhiteSpace(json) || json == "null" ? null : json;
	}

	/// <summary>The raw JSON; <c>null</c> for the empty value.</summary>
	public string Json => _json ?? "null";

	public bool IsNull => _json is null;

	public static ParameterValue Null => default;

	public static ParameterValue FromString(string? value) =>
		value is null ? default : new ParameterValue(JsonSerializer.Serialize(value));

	public static ParameterValue FromBoolean(bool value) => new(value ? "true" : "false");

	public static ParameterValue FromNumber(decimal value) =>
		new(value.ToString(CultureInfo.InvariantCulture));

	public static ParameterValue FromStringList(IEnumerable<string> values) =>
		new(JsonSerializer.Serialize(values.ToArray()));

	public static ParameterValue FromPairs(IEnumerable<KeyValuePair<string, string>> pairs)
	{
		var buffer = new ArrayBufferWriter<byte>();

		using (var writer = new Utf8JsonWriter(buffer))
		{
			writer.WriteStartObject();

			foreach (var (key, value) in pairs)
			{
				writer.WriteString(key, value);
			}

			writer.WriteEndObject();
		}

		return new ParameterValue(System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan));
	}

	/// <summary>Text of the value: strings unquoted, numbers and booleans as written, anything else as JSON.</summary>
	public string AsString(string fallback = "")
	{
		if (IsNull)
		{
			return fallback;
		}

		try
		{
			using var document = JsonDocument.Parse(Json);
			var root = document.RootElement;

			return root.ValueKind switch
			{
				JsonValueKind.String => root.GetString() ?? fallback,
				_ => root.GetRawText(),
			};
		}
		catch (JsonException)
		{
			return Json;
		}
	}

	public bool AsBoolean(bool fallback = false)
	{
		if (IsNull)
		{
			return fallback;
		}

		try
		{
			using var document = JsonDocument.Parse(Json);

			return document.RootElement.ValueKind switch
			{
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				JsonValueKind.String => bool.TryParse(document.RootElement.GetString(), out var parsed) ? parsed : fallback,
				_ => fallback,
			};
		}
		catch (JsonException)
		{
			return fallback;
		}
	}

	/// <summary>The number the value holds, or <c>null</c> if it is empty or not a number.</summary>
	public decimal? AsDecimal()
	{
		if (IsNull)
		{
			return null;
		}

		try
		{
			using var document = JsonDocument.Parse(Json);
			var root = document.RootElement;

			return root.ValueKind switch
			{
				JsonValueKind.Number when root.TryGetDecimal(out var number) => number,
				JsonValueKind.String when decimal.TryParse(
					root.GetString(),
					NumberStyles.Number,
					CultureInfo.InvariantCulture,
					out var parsed) => parsed,
				_ => null,
			};
		}
		catch (JsonException)
		{
			return null;
		}
	}

	public IReadOnlyList<string> AsStringList()
	{
		if (IsNull)
		{
			return [];
		}

		try
		{
			using var document = JsonDocument.Parse(Json);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
			{
				return [];
			}

			return
			[
				.. document.RootElement.EnumerateArray().Select(item =>
					item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.GetRawText()),
			];
		}
		catch (JsonException)
		{
			return [];
		}
	}

	public IReadOnlyList<KeyValuePair<string, string>> AsPairs()
	{
		if (IsNull)
		{
			return [];
		}

		try
		{
			using var document = JsonDocument.Parse(Json);

			if (document.RootElement.ValueKind != JsonValueKind.Object)
			{
				return [];
			}

			return
			[
				.. document.RootElement.EnumerateObject().Select(property => new KeyValuePair<string, string>(
					property.Name,
					property.Value.ValueKind == JsonValueKind.String
						? property.Value.GetString() ?? string.Empty
						: property.Value.GetRawText())),
			];
		}
		catch (JsonException)
		{
			return [];
		}
	}

	public override string ToString() => Json;
}
