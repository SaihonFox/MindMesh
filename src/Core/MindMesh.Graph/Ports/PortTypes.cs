namespace MindMesh.Graph.Ports;

public static class PortTypes
{
	/// <summary>
	/// Returns the visual family of a port type.
	/// </summary>
	public static PortKind GetKind(PortType type) => type switch
	{
		PortType.Execution => PortKind.Execution,
		PortType.Event => PortKind.Event,
		PortType.AIContext => PortKind.AIContext,
		_ => PortKind.Data,
	};
}
