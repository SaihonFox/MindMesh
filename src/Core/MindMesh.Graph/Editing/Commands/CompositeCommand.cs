namespace MindMesh.Graph.Editing.Commands;

/// <summary>
/// Several commands recorded as one undo step.
/// </summary>
public sealed class CompositeCommand(string name, IEnumerable<IGraphCommand> commands) : IGraphCommand
{
	private readonly List<IGraphCommand> _commands = [.. commands];

	public string Name { get; } = name;

	public bool IsEmpty => _commands.Count == 0;

	public void Execute(GraphDocument document)
	{
		foreach (var command in _commands)
		{
			command.Execute(document);
		}
	}

	public void Undo(GraphDocument document)
	{
		for (var index = _commands.Count - 1; index >= 0; index--)
		{
			_commands[index].Undo(document);
		}
	}
}
