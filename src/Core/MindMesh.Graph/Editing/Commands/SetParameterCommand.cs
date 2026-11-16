using MindMesh.Graph.Definitions;
using MindMesh.Graph.Identifiers;

namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Changes a setting. Consecutive changes of the same setting are merged into one undo step.
/// </summary>
public sealed class SetParameterCommand(NodeId node, string key, ParameterValue newValue) : IMergeableCommand
{
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

		document.Graph.SetParameter(Node, Key, NewValue);
	}

	public void Undo(GraphDocument document) => document.Graph.SetParameter(Node, Key, _old);

	public bool TryMerge(IGraphCommand next)
	{
		if (next is not SetParameterCommand other || other.Node != Node || other.Key != Key)
		{
			return false;
		}

		NewValue = other.NewValue;
		return true;
	}
}
