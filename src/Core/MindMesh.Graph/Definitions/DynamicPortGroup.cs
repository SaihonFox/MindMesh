using System.Globalization;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Definitions;

/// <summary>
/// A group of output ports whose number the user chooses with a setting, like the cases of a switch.
/// Ports are numbered from 1; the key and the name of a port are made from the formats ("case{0}", "Case {0}").
/// </summary>
/// <param name="CountParameter">The number setting that holds how many ports the group has.</param>
/// <param name="KeyFormat">Format of the port key; the number is the argument.</param>
/// <param name="NameFormat">Format of the port name; the number is the argument.</param>
public sealed record DynamicPortGroup(
	string CountParameter,
	string KeyFormat,
	string NameFormat,
	PortType Type,
	int DefaultCount,
	int MinimumCount,
	int MaximumCount)
{
	/// <summary>The number of ports for the settings of a node: the setting, kept between the minimum and the maximum.</summary>
	public int CountOf(IReadOnlyDictionary<string, ParameterValue> values)
	{
		int count = values.TryGetValue(CountParameter, out ParameterValue value) && value.AsDecimal() is { } number
			? (int)Math.Round(number)
			: DefaultCount;

		return Math.Clamp(count, MinimumCount, MaximumCount);
	}

	/// <summary>The ports for a count.</summary>
	public IReadOnlyList<PortDefinition> Create(int count) =>
	[
		.. Enumerable.Range(1, count).Select(number => new PortDefinition
		{
			Key = KeyOf(number),
			Name = string.Format(CultureInfo.InvariantCulture, NameFormat, number),
			Direction = PortDirection.Output,
			Type = Type,
			AllowMultiple = true,
		}),
	];

	public string KeyOf(int number) => string.Format(CultureInfo.InvariantCulture, KeyFormat, number);

	/// <summary>The number a port key stands for, or null when the key does not belong to the group.</summary>
	public int? NumberOf(string key)
	{
		for (int number = 1; number <= MaximumCount; number++)
		{
			if (KeyOf(number) == key)
				return number;
		}

		return null;
	}
}
