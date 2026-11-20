using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Operations;

/// <summary>
/// One change of a graph, described as plain data: it can be stored, sent and applied later.
/// The assistant and plugins change graphs through these. Nodes are named by identifiers the author chooses,
/// so a batch can add a node and connect it in one go.
/// </summary>
public abstract record GraphOperation;

/// <summary>Adds a node of a known type. The identifier is chosen by the author of the batch.</summary>
public sealed record AddNodeOperation(
	NodeId Node,
	NodeDefinitionId Type,
	double X,
	double Y,
	string? Title = null,
	IReadOnlyDictionary<string, string>? Parameters = null) : GraphOperation;

public sealed record RemoveNodeOperation(NodeId Node) : GraphOperation;

public sealed record ConnectOperation(NodeId SourceNode, string SourcePort, NodeId TargetNode, string TargetPort) : GraphOperation;

public sealed record DisconnectOperation(NodeId SourceNode, string SourcePort, NodeId TargetNode, string TargetPort) : GraphOperation;

/// <summary>Sets a setting. The value is JSON text, like in the workflow file.</summary>
public sealed record SetParameterOperation(NodeId Node, string Key, string ValueJson) : GraphOperation;

public sealed record SetTitleOperation(NodeId Node, string? Title) : GraphOperation;

public sealed record SetDisabledOperation(NodeId Node, bool IsDisabled) : GraphOperation;

public sealed record MoveNodeOperation(NodeId Node, double X, double Y) : GraphOperation;

/// <summary>Operations that are applied together or not at all.</summary>
public sealed record GraphOperationBatch(string Name, IReadOnlyList<GraphOperation> Operations);
