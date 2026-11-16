using MindMesh.Plugins.Abstractions.Projects;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// One entry of the editor mode switcher.
/// </summary>
public sealed class EditorModeOptionViewModel : ViewModelBase
{
	private readonly NodeText _text;

	public EditorModeOptionViewModel(EditorModeDefinition mode, NodeText text, ILocalizer localizer)
		: base(localizer)
	{
		Definition = mode;
		_text = text;
	}

	public EditorModeDefinition Definition { get; }

	public EditorModeId Mode => Definition.Id;

	public string Title => _text.GetModeTitle(Definition);

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(Title));
}
