using MindMesh.Graph.Editing;

namespace MindMesh.Presentation.Pages.Editor.Canvas;

/// <summary>
/// The clipboard of the graph editor. It lives inside the application, so copied nodes keep their exact
/// settings and survive between workflows; secrets are never copied.
/// </summary>
public sealed class GraphClipboard
{
	public GraphFragment? Fragment { get; private set; }

	/// <summary>How many times the current content was pasted; each paste shifts the copies a little further.</summary>
	public int PasteCount { get; private set; }

	public bool HasContent => Fragment is { IsEmpty: false };

	public void Set(GraphFragment fragment)
	{
		Fragment = fragment;
		PasteCount = 0;
	}

	public int NextPasteIndex() => ++PasteCount;
}
