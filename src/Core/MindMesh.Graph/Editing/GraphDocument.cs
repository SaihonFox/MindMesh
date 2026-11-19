using MindMesh.Graph.Layout;
using MindMesh.Graph.Model;

namespace MindMesh.Graph.Editing;

/// <summary>
/// A graph together with its layout and the undo history. All user edits go through <see cref="Execute"/>.
/// </summary>
public sealed class GraphDocument
{
	public const int DefaultHistoryLimit = 200;

	private readonly List<IGraphCommand> _undo = [];
	private readonly List<IGraphCommand> _redo = [];
	private readonly int _historyLimit;

	public GraphDocument(NodeGraph? graph = null, GraphLayout? layout = null, int historyLimit = DefaultHistoryLimit)
	{
		ArgumentOutOfRangeException.ThrowIfLessThan(historyLimit, 1);

		Graph = graph ?? new NodeGraph();
		Layout = layout ?? new GraphLayout();
		_historyLimit = historyLimit;
	}

	public NodeGraph Graph { get; }

	public GraphLayout Layout { get; }

	public bool CanUndo => _undo.Count > 0;

	public bool CanRedo => _redo.Count > 0;

	/// <summary>Name of the command Undo would revert.</summary>
	public string? UndoName => _undo.Count > 0 ? _undo[^1].Name : null;

	public string? RedoName => _redo.Count > 0 ? _redo[^1].Name : null;

	/// <summary>Raised when the undo or redo availability may have changed.</summary>
	public event EventHandler? HistoryChanged;

	/// <summary>Runs a command and records it, or merges it into the previous one.</summary>
	public void Execute(IGraphCommand command)
	{
		ArgumentNullException.ThrowIfNull(command);

		command.Execute(this);
		_redo.Clear();

		if (_undo.Count > 0 && _undo[^1] is IMergeableCommand previous && previous.TryMerge(command))
		{
			RaiseHistoryChanged();
			return;
		}

		_undo.Add(command);

		if (_undo.Count > _historyLimit)
			_undo.RemoveAt(0);

		RaiseHistoryChanged();
	}

	public void Undo()
	{
		if (_undo.Count == 0)
			return;

		var command = _undo[^1];
		_undo.RemoveAt(_undo.Count - 1);
		command.Undo(this);
		_redo.Add(command);
		RaiseHistoryChanged();
	}

	public void Redo()
	{
		if (_redo.Count == 0)
			return;

		var command = _redo[^1];
		_redo.RemoveAt(_redo.Count - 1);
		command.Execute(this);
		_undo.Add(command);
		RaiseHistoryChanged();
	}

	public void ClearHistory()
	{
		_undo.Clear();
		_redo.Clear();
		RaiseHistoryChanged();
	}

	private void RaiseHistoryChanged() => HistoryChanged?.Invoke(this, EventArgs.Empty);
}
