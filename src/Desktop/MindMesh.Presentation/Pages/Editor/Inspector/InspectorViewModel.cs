using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Secrets;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Nodes;
using MindMesh.Presentation.Platform;

namespace MindMesh.Presentation.Pages.Editor.Inspector;

/// <summary>
/// Inspector panel. Shows the settings of the selected node, a summary when several nodes are selected,
/// and a hint when nothing is.
/// </summary>
public sealed partial class InspectorViewModel : ViewModelBase
{
	private readonly GraphEditorViewModel _editor;
	private readonly NodeText _text;
	private readonly ISecretStore _secrets;
	private readonly IFilePickerService _picker;

	[ObservableProperty]
	private NodeInspectorViewModel? _node;

	public InspectorViewModel(
		ILocalizer localizer,
		GraphEditorViewModel editor,
		NodeText text,
		ISecretStore secrets,
		IFilePickerService picker)
		: base(localizer)
	{
		_editor = editor;
		_text = text;
		_secrets = secrets;
		_picker = picker;

		editor.SelectionChanged += (_, _) => Rebuild();
		Rebuild();
	}

	public int SelectionCount => _editor.SelectedNodes.Count;

	public bool HasNode => Node is not null;

	public bool IsMultipleSelection => SelectionCount > 1;

	public bool IsEmpty => SelectionCount == 0;

	public string MultipleSelectionText => string.Format(Loc.Culture, Loc["Inspector_MultipleSelected"], SelectionCount);

	protected override void OnLanguageChanged() => OnPropertyChanged(nameof(MultipleSelectionText));

	private void Rebuild()
	{
		Node?.Dispose();

		Node = _editor.SelectedNodes.Count == 1
			? new NodeInspectorViewModel(_editor.SelectedNodes[0], _editor, _text, _secrets, _picker, Loc)
			: null;

		OnPropertyChanged(nameof(SelectionCount));
		OnPropertyChanged(nameof(HasNode));
		OnPropertyChanged(nameof(IsMultipleSelection));
		OnPropertyChanged(nameof(IsEmpty));
		OnPropertyChanged(nameof(MultipleSelectionText));
	}
}
