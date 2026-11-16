using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Ports;

/// <summary>
/// Points to a port of a node instance.
/// </summary>
public readonly record struct PortReference(NodeId Node, string PortKey)
{
	public override string ToString() => $"{Node}:{PortKey}";
}
