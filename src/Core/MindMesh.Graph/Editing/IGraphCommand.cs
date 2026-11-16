namespace MindMesh.Graph.Editing;

/// <summary>
/// A reversible change of a <see cref="GraphDocument"/>. <see cref="Execute"/> must be repeatable
/// (redo runs it again) and <see cref="Undo"/> must restore exactly the previous state.
/// </summary>
public interface IGraphCommand
{
	string Name { get; }

	void Execute(GraphDocument document);

	void Undo(GraphDocument document);
}

/// <summary>
/// A command that can absorb the command that follows it, so a burst of edits (typing in a field)
/// becomes one undo step.
/// </summary>
public interface IMergeableCommand : IGraphCommand
{
	/// <summary>Absorbs <paramref name="next"/> into this command. Returns false if they do not belong together.</summary>
	bool TryMerge(IGraphCommand next);
}
