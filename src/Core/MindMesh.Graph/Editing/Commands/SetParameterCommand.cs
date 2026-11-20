using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;
using MindMesh.Graph.Model;
using MindMesh.Graph.Ports;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Changes a setting. Consecutive changes of the same setting are merged into one undo step.
/// </summary>
public sealed class SetParameterCommand(NodeId node, string key, ParameterValue newValue) : IMergeableCommand
{
	private readonly List<Connection> _removed = [];
	private ParameterValue _old;
	private bool _captured;

	public string Name => "Change setting";

	public NodeId Node { get; } = node;

	public string Key { get; } = key;

	public ParameterValue NewValue { get; private set; } = newValue;

	public void Execute(GraphDocument document)
	{
		if (!_captured)
		{
			_old = document.Graph.GetNode(Node).GetParameter(Key);
			_captured = true;
		}

		// Lowering the number of dynamic ports (the cases of a switch) removes the connections of the ports that go away.
		// They are taken out first, while their ports still exist, and put back by undo.
		_removed.Clear();

		NodeInstance node = document.Graph.GetNode(Node);
		HashSet<string> vanishing = [.. VanishingPortKeys(node)];

		foreach (Connection connection in node.Outputs.SelectMany(port => document.Graph.GetConnections(new PortReference(Node, port.Key))).Where(connection => vanishing.Contains(connection.Source.PortKey)).ToList())
		{
			_removed.Add(connection);
			document.Graph.RemoveConnection(connection.Id);
		}

		document.Graph.SetParameter(Node, Key, NewValue);
	}

	public void Undo(GraphDocument document)
	{
		document.Graph.SetParameter(Node, Key, _old);

		foreach (Connection connection in _removed)
			document.Graph.AddConnection(connection);
	}

	public bool TryMerge(IGraphCommand next)
	{
		if (next is not SetParameterCommand other || other.Node != Node || other.Key != Key)
			return false;

		NewValue = other.NewValue;
		_removed.AddRange(other._removed);
		return true;
	}

	private IEnumerable<string> VanishingPortKeys(NodeInstance node)
	{
		Dictionary<string, ParameterValue> changed = new(node.Parameters) { [Key] = NewValue };

		foreach (DynamicPortGroup group in node.Definition.DynamicOutputs.Where(group => group.CountParameter == Key))
		{
			for (int number = group.CountOf(changed) + 1; number <= group.CountOf(node.Parameters); number++)
				yield return group.KeyOf(number);
		}
	}
}
