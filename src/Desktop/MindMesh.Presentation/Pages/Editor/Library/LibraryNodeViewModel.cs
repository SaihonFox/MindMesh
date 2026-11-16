using MindMesh.Graph.Definitions;
using MindMesh.Plugins.Abstractions.Nodes;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Library;

/// <summary>
/// A node type as it appears in the library and in the quick-add list. Created fresh whenever the list is rebuilt,
/// so the texts are plain values.
/// </summary>
public sealed class LibraryNodeViewModel
{
	public LibraryNodeViewModel(INodeDefinition definition, NodeText text)
	{
		Definition = definition;
		Title = text.GetName(definition);
		Description = text.GetDescription(definition);
		CategoryTitle = text.GetPrimaryCategoryTitle(definition);
		Tone = text.GetTone(definition);
	}

	public INodeDefinition Definition { get; }

	public string Title { get; }

	public string Description { get; }

	public string CategoryTitle { get; }

	public string IconKey => Definition.IconKey;

	public AccentTone Tone { get; }
}
