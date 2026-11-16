using CommunityToolkit.Mvvm.ComponentModel;
using MindMesh.Application.Extensions;
using MindMesh.Presentation.Common;
using MindMesh.Presentation.Localization;
using MindMesh.Presentation.Pages.Editor.Canvas;
using MindMesh.Presentation.Pages.Editor.Nodes;

namespace MindMesh.Presentation.Pages.Editor;

/// <summary>
/// State of the editor toolbar in the top bar: mode switcher, panel toggles and editing actions.
/// </summary>
public sealed partial class EditorToolbarViewModel : ViewModelBase
{
	private readonly EditorWorkspace _workspace;

	[ObservableProperty]
	private EditorModeOptionViewModel? _selectedMode;

	public EditorToolbarViewModel(
		ILocalizer localizer,
		EditorWorkspace workspace,
		EditorLayoutViewModel layout,
		GraphEditorViewModel editor,
		IExtensionCatalog catalog,
		NodeText text)
		: base(localizer)
	{
		_workspace = workspace;
		Layout = layout;
		Editor = editor;

		var kind = catalog.FindProjectKind(workspace.ProjectKind);

		Modes = [.. (kind?.Modes ?? []).Select(mode => new EditorModeOptionViewModel(mode, text, localizer))];
		_selectedMode = Modes.FirstOrDefault(option => option.Mode == workspace.Mode);

		workspace.PropertyChanged += (_, e) =>
		{
			if (e.PropertyName == nameof(EditorWorkspace.Mode))
			{
				SelectedMode = Modes.FirstOrDefault(option => option.Mode == _workspace.Mode);
			}
		};
	}

	public EditorLayoutViewModel Layout { get; }

	/// <summary>The graph editor, for undo and redo.</summary>
	public GraphEditorViewModel Editor { get; }

	public IReadOnlyList<EditorModeOptionViewModel> Modes { get; }

	/// <summary>
	/// True when a workflow is open in the editor. Validate, run, debug and auto layout need the workflow core
	/// (Phase 3 onwards) and stay disabled until then.
	/// </summary>
	public bool HasActiveWorkflow => false;

	partial void OnSelectedModeChanged(EditorModeOptionViewModel? value)
	{
		if (value is not null)
		{
			_workspace.Mode = value.Mode;
		}
	}
}
