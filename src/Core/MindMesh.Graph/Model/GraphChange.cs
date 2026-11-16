using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Model;

/// <summary>
/// One change of the graph definition. Views apply these incrementally instead of rebuilding everything.
/// </summary>
public abstract record GraphChange;

public sealed record NodeAdded(NodeInstance Node) : GraphChange;

public sealed record NodeRemoved(NodeInstance Node) : GraphChange;

public sealed record ConnectionAdded(Connection Connection) : GraphChange;

public sealed record ConnectionRemoved(Connection Connection) : GraphChange;

public sealed record ParameterChanged(NodeId Node, string Key, ParameterValue OldValue, ParameterValue NewValue) : GraphChange;

public sealed record TitleChanged(NodeId Node, string? OldTitle, string? NewTitle) : GraphChange;

public sealed record DisabledChanged(NodeId Node, bool IsDisabled) : GraphChange;

public sealed class GraphChangedEventArgs(GraphChange change) : EventArgs
{
	public GraphChange Change { get; } = change;
}
