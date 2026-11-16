using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Graph.Identifiers;
using MindMesh.Plugins.Abstractions.Nodes;

namespace MindMesh.Presentation.Pages.Editor.Library;

/// <summary>
/// A collapsible category of the node library.
/// </summary>
public sealed partial class LibrarySectionViewModel : ObservableObject
{
	[ObservableProperty]
	private bool _isExpanded;

	public LibrarySectionViewModel(
		NodeCategoryId id,
		string title,
		string iconKey,
		AccentTone tone,
		IReadOnlyList<LibraryNodeViewModel> items,
		bool isExpanded)
	{
		Id = id;
		Title = title;
		IconKey = iconKey;
		Tone = tone;
		Items = items;
		_isExpanded = isExpanded;
	}

	public NodeCategoryId Id { get; }

	public string Title { get; }

	public string IconKey { get; }

	public AccentTone Tone { get; }

	public IReadOnlyList<LibraryNodeViewModel> Items { get; }

	public int Count => Items.Count;
}
