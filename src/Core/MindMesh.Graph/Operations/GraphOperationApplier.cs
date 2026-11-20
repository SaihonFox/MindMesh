using System.Text.Json;
using MindMesh.Graph.Definitions;
using MindMesh.Graph.Editing;
using MindMesh.Graph.Editing.Commands;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Operations;

/// <summary>
/// Checks and applies batches of operations. A batch is atomic: it is checked as a whole first, and only a batch
/// without problems changes the document, as one undo step.
/// </summary>
public sealed class GraphOperationApplier(Func<NodeDefinitionId, INodeDefinition?> findDefinition)
{
	/// <summary>Finds every problem of the batch without changing the document.</summary>
	public GraphOperationResult Validate(GraphDocument document, GraphOperationBatch batch)
	{
		ArgumentNullException.ThrowIfNull(document);
		ArgumentNullException.ThrowIfNull(batch);

		List<GraphOperationFailure> failures = [];
		Dictionary<NodeId, INodeDefinition> added = [];
		HashSet<NodeId> removed = [];

		for (int index = 0; index < batch.Operations.Count; index++)
		{
			if (Check(document, batch.Operations[index], added, removed) is { } problem)
				failures.Add(new GraphOperationFailure(index, problem.Problem, problem.Detail));
		}

		return new GraphOperationResult(failures);
	}

	/// <summary>Applies the batch when it has no problems. Returns the result; on failure nothing changed.</summary>
	public GraphOperationResult Apply(GraphDocument document, GraphOperationBatch batch)
	{
		GraphOperationResult result = Validate(document, batch);

		if (!result.IsSuccess || batch.Operations.Count == 0)
			return result;

		AtomicCommand command = new(batch.Name, [.. batch.Operations.Select(operation => ToCommand(document, operation))]);

		try
		{
			document.Execute(command);
		}
		catch (InvalidOperationException exception)
		{
			// The check could not foresee everything (for example a loop made by two connections of the same batch).
			return new GraphOperationResult([new GraphOperationFailure(command.FailedIndex, GraphOperationProblem.ConnectionNotAllowed, exception.Message)]);
		}

		return result;
	}

	private (GraphOperationProblem Problem, string? Detail)? Check(
		GraphDocument document,
		GraphOperation operation,
		Dictionary<NodeId, INodeDefinition> added,
		HashSet<NodeId> removed)
	{
		INodeDefinition? DefinitionOf(NodeId id)
		{
			if (removed.Contains(id))
				return null;

			return added.TryGetValue(id, out INodeDefinition? pending) ? pending : document.Graph.FindNode(id)?.Definition;
		}

		switch (operation)
		{
			case AddNodeOperation add:
				if (findDefinition(add.Type) is not { } definition)
					return (GraphOperationProblem.UnknownNodeType, add.Type.Value);

				if (DefinitionOf(add.Node) is not null || add.Node.IsEmpty)
					return (GraphOperationProblem.NodeAlreadyExists, add.Node.ToString());

				foreach ((string key, string json) in add.Parameters ?? new Dictionary<string, string>())
				{
					if (definition.Parameters.All(parameter => parameter.Key != key))
						return (GraphOperationProblem.UnknownSetting, key);

					if (!IsJson(json))
						return (GraphOperationProblem.InvalidValue, key);
				}

				added[add.Node] = definition;
				removed.Remove(add.Node);

				return null;

			case RemoveNodeOperation remove:
				if (DefinitionOf(remove.Node) is null)
					return (GraphOperationProblem.UnknownNode, remove.Node.ToString());

				added.Remove(remove.Node);
				removed.Add(remove.Node);

				return null;

			case ConnectOperation connect:
				return CheckPorts(DefinitionOf(connect.SourceNode), connect.SourceNode, connect.SourcePort, DefinitionOf(connect.TargetNode), connect.TargetNode, connect.TargetPort, document, added.Count > 0);

			case DisconnectOperation disconnect:
				if (DefinitionOf(disconnect.SourceNode) is null || DefinitionOf(disconnect.TargetNode) is null)
					return (GraphOperationProblem.UnknownNode, null);

				return FindConnection(document, disconnect.SourceNode, disconnect.SourcePort, disconnect.TargetNode, disconnect.TargetPort) is null && added.Count == 0
					? (GraphOperationProblem.ConnectionNotFound, null)
					: null;

			case SetParameterOperation set:
				if (DefinitionOf(set.Node) is not { } owner)
					return (GraphOperationProblem.UnknownNode, set.Node.ToString());

				if (owner.Parameters.All(parameter => parameter.Key != set.Key))
					return (GraphOperationProblem.UnknownSetting, set.Key);

				return IsJson(set.ValueJson) ? null : (GraphOperationProblem.InvalidValue, set.Key);

			case SetTitleOperation title:
				return DefinitionOf(title.Node) is null ? (GraphOperationProblem.UnknownNode, title.Node.ToString()) : null;

			case SetDisabledOperation disabled:
				return DefinitionOf(disabled.Node) is null ? (GraphOperationProblem.UnknownNode, disabled.Node.ToString()) : null;

			case MoveNodeOperation move:
				return DefinitionOf(move.Node) is null ? (GraphOperationProblem.UnknownNode, move.Node.ToString()) : null;

			default:
				return (GraphOperationProblem.InvalidValue, operation.GetType().Name);
		}
	}

	private static (GraphOperationProblem, string?)? CheckPorts(
		INodeDefinition? source,
		NodeId sourceNode,
		string sourcePort,
		INodeDefinition? target,
		NodeId targetNode,
		string targetPort,
		GraphDocument document,
		bool hasPendingNodes)
	{
		if (source is null)
			return (GraphOperationProblem.UnknownNode, sourceNode.ToString());

		if (target is null)
			return (GraphOperationProblem.UnknownNode, targetNode.ToString());

		if (source.FindPort(sourcePort, PortDirection.Output) is not { } output)
			return (GraphOperationProblem.UnknownPort, sourcePort);

		if (target.FindPort(targetPort, PortDirection.Input) is not { } input)
			return (GraphOperationProblem.UnknownPort, targetPort);

		if (sourceNode == targetNode || !PortCompatibility.IsCompatible(output.Type, input.Type))
			return (GraphOperationProblem.ConnectionNotAllowed, null);

		// Loops and duplicates among nodes that exist already are caught here; the rest is checked when it runs.
		if (!hasPendingNodes && !document.Graph.CheckConnection(new PortReference(sourceNode, sourcePort), new PortReference(targetNode, targetPort)).IsAllowed)
			return (GraphOperationProblem.ConnectionNotAllowed, null);

		return null;
	}

	private static Connection? FindConnection(GraphDocument document, NodeId source, string sourcePort, NodeId target, string targetPort) =>
		document.Graph.GetConnections(new PortReference(source, sourcePort))
			.FirstOrDefault(connection => connection.Target == new PortReference(target, targetPort));

	private IGraphCommand ToCommand(GraphDocument document, GraphOperation operation) => operation switch
	{
		AddNodeOperation add => new AddNodeCommand(
			new NodeInstance(
				add.Node,
				findDefinition(add.Type)!,
				add.Title,
				parameters: add.Parameters?.Select(pair => KeyValuePair.Create(pair.Key, new ParameterValue(pair.Value)))),
			new NodePlacement(add.X, add.Y)),
		RemoveNodeOperation remove => new RemoveNodesCommand([remove.Node]),
		ConnectOperation connect => new ConnectCommand(
			new PortReference(connect.SourceNode, connect.SourcePort),
			new PortReference(connect.TargetNode, connect.TargetPort)),
		DisconnectOperation disconnect => new LazyDisconnectCommand(disconnect),
		SetParameterOperation set => new SetParameterCommand(set.Node, set.Key, new ParameterValue(set.ValueJson)),
		SetTitleOperation title => new SetTitleCommand(title.Node, title.Title),
		SetDisabledOperation disabled => new SetDisabledCommand([disabled.Node], disabled.IsDisabled),
		MoveNodeOperation move => new LazyMoveCommand(move),
		_ => throw new InvalidOperationException($"Unknown operation {operation.GetType().Name}."),
	};

	private static bool IsJson(string text)
	{
		try
		{
			using JsonDocument _ = JsonDocument.Parse(text);

			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}

	/// <summary>Runs its commands in order; when one throws, the ones that ran are undone, so the document stays as it was.</summary>
	private sealed class AtomicCommand(string name, List<IGraphCommand> commands) : IGraphCommand
	{
		public string Name { get; } = name;

		/// <summary>The position of the command that failed last.</summary>
		public int FailedIndex { get; private set; }

		public void Execute(GraphDocument document)
		{
			int done = 0;

			try
			{
				for (; done < commands.Count; done++)
					commands[done].Execute(document);
			}
			catch
			{
				FailedIndex = done;

				for (int index = done - 1; index >= 0; index--)
					commands[index].Undo(document);

				throw;
			}
		}

		public void Undo(GraphDocument document)
		{
			for (int index = commands.Count - 1; index >= 0; index--)
				commands[index].Undo(document);
		}
	}

	/// <summary>Removes a connection found by its ports when it runs, because the connection may be created earlier in the same batch.</summary>
	private sealed class LazyDisconnectCommand(DisconnectOperation operation) : IGraphCommand
	{
		private DisconnectCommand? _inner;

		public string Name => "Disconnect";

		public void Execute(GraphDocument document)
		{
			ConnectionId? id = FindConnection(document, operation.SourceNode, operation.SourcePort, operation.TargetNode, operation.TargetPort)?.Id;

			_inner = new DisconnectCommand(id is { } found ? [found] : []);
			_inner.Execute(document);
		}

		public void Undo(GraphDocument document) => _inner?.Undo(document);
	}

	/// <summary>Moves a node from wherever it is when the command runs.</summary>
	private sealed class LazyMoveCommand(MoveNodeOperation operation) : IGraphCommand
	{
		private MoveNodesCommand? _inner;

		public string Name => "Move";

		public void Execute(GraphDocument document)
		{
			_inner ??= new MoveNodesCommand([new NodeMove(operation.Node, document.Layout.GetPlacement(operation.Node), new NodePlacement(operation.X, operation.Y))]);
			_inner.Execute(document);
		}

		public void Undo(GraphDocument document) => _inner?.Undo(document);
	}
}
