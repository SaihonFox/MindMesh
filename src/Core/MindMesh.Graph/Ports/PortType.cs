namespace MindMesh.Graph.Ports;

/// <summary>
/// Type of the values a port carries. <see cref="Execution"/>, <see cref="Event"/> and <see cref="AIContext"/>
/// are control types that only connect to ports of the same type.
/// </summary>
public enum PortType
{
	Any,
	String,
	Number,
	Integer,
	Decimal,
	Boolean,
	Object,
	Array,
	Json,
	DateTime,
	Binary,
	Execution,
	Event,
	AIContext,
}
