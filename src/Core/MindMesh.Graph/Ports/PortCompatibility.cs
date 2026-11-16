namespace MindMesh.Graph.Ports;

/// <summary>
/// Rules that decide which output may feed which input.
/// </summary>
public static class PortCompatibility
{
	/// <summary>Implicit conversions between different data types (source, target).</summary>
	private static readonly HashSet<(PortType Source, PortType Target)> Conversions =
	[
		(PortType.Integer, PortType.Number),
		(PortType.Decimal, PortType.Number),
		(PortType.Number, PortType.Decimal),
		(PortType.Integer, PortType.Decimal),
		(PortType.Integer, PortType.String),
		(PortType.Number, PortType.String),
		(PortType.Decimal, PortType.String),
		(PortType.Boolean, PortType.String),
		(PortType.DateTime, PortType.String),
		(PortType.Object, PortType.String),
		(PortType.Array, PortType.String),
		(PortType.Json, PortType.String),
		(PortType.String, PortType.Json),
		(PortType.Object, PortType.Json),
		(PortType.Array, PortType.Json),
		(PortType.Json, PortType.Object),
		(PortType.Json, PortType.Array),
	];

	/// <summary>
	/// True if a value of <paramref name="source"/> may be passed to a port of <paramref name="target"/>.
	/// </summary>
	public static bool IsCompatible(PortType source, PortType target)
	{
		if (source == target)
		{
			return true;
		}

		var kind = PortTypes.GetKind(source);

		if (kind != PortTypes.GetKind(target) || kind != PortKind.Data)
		{
			return false;
		}

		if (source == PortType.Any || target == PortType.Any)
		{
			return true;
		}

		return Conversions.Contains((source, target));
	}

	/// <summary>
	/// True if the value has to be converted on its way (the types differ and neither is <see cref="PortType.Any"/>).
	/// </summary>
	public static bool RequiresConversion(PortType source, PortType target) =>
		source != target
		&& source != PortType.Any
		&& target != PortType.Any
		&& IsCompatible(source, target);
}
