using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Model;

/// <summary>
/// A link from an output port (<see cref="Source"/>) to an input port (<see cref="Target"/>).
/// </summary>
public sealed record Connection(ConnectionId Id, PortReference Source, PortReference Target);
